#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.IO;
using DotNetSourceGeneratorToolkit.Exceptions;

namespace DotNetSourceGeneratorToolkit.Infrastructure;

/// <summary>
/// Provides a retry policy for handling transient file system failures with exponential backoff.
/// Retries on IOException, UnauthorizedAccessException, and sharing violations (HRESULT 0x80070020).
/// Does not retry on DirectoryNotFoundException, FileNotFoundException, or path format errors.
/// </summary>
public sealed class RetryPolicy : IRetryPolicy
{
    private const int MaxAttempts = 3;
    private readonly int _maxAttempts;
    private readonly bool _enableLogging;

    /// <summary>
    /// Initializes a new instance of the <see cref="RetryPolicy"/> class.
    /// </summary>
    /// <param name="maxAttempts">Maximum number of retry attempts. Default is 3.</param>
    /// <param name="enableLogging">Whether to log retry attempts. Default is true.</param>
    public RetryPolicy(int maxAttempts = MaxAttempts, bool enableLogging = true)
    {
        if (maxAttempts < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(maxAttempts), "Max attempts must be at least 1");
        }

        _maxAttempts = maxAttempts;
        _enableLogging = enableLogging;
    }

    /// <summary>
    /// Determines whether a given exception represents a transient error that should be retried.
    /// </summary>
    /// <param name="exception">The exception to evaluate.</param>
    /// <returns>True if the exception is transient and should be retried; otherwise, false.</returns>
    public bool IsTransientError(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        // Don't retry on these - they indicate permanent issues
        if (exception is DirectoryNotFoundException or FileNotFoundException or PathTooLongException)
        {
            return false;
        }

        // Retry on IOException (including sharing violations)
        if (exception is IOException)
        {
            return true;
        }

        // Retry on UnauthorizedAccessException (common when antivirus temporarily locks files)
        if (exception is UnauthorizedAccessException)
        {
            return true;
        }

        // Check for sharing violation HRESULT (0x80070020)
        if (exception is System.Runtime.InteropServices.COMException comEx &&
            comEx.HResult == unchecked((int)0x80070020))
        {
            return true;
        }

        // Check for sharing violation HRESULT in Win32Exception
        if (exception is System.ComponentModel.Win32Exception win32Ex &&
            win32Ex.NativeErrorCode == 0x20) // ERROR_SHARING_VIOLATION
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Gets the delay duration for a retry attempt using exponential backoff.
    /// Attempt 1: 50ms
    /// Attempt 2: 150ms
    /// Attempt 3+: 450ms
    /// </summary>
    /// <param name="attempt">The retry attempt number (1-based).</param>
    /// <returns>The delay duration before the next retry attempt.</returns>
    public TimeSpan GetDelay(int attempt)
    {
        if (attempt < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(attempt), "Attempt must be at least 1");
        }

        // Exponential backoff with jitter
        var baseDelay = attempt switch
        {
            1 => 50,
            2 => 150,
            _ => 450
        };

        // Add jitter to prevent thundering herd
        var random = new Random();
        var jitter = random.Next(0, 50);
        var totalDelayMs = baseDelay + jitter;

        return TimeSpan.FromMilliseconds(totalDelayMs);
    }

    /// <summary>
    /// Executes an action with retry logic.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <param name="filePath">The file path associated with the operation (for error reporting).</param>
    /// <returns>A task representing the operation.</returns>
    public async Task ExecuteAsync(Func<Task> action, string filePath)
    {
        ArgumentNullException.ThrowIfNull(action);
        ArgumentException.ThrowIfNullOrEmpty(filePath);

        var attempts = 0;
        Exception? lastException = null;

        while (attempts < _maxAttempts)
        {
            attempts++;

            try
            {
                await action();
                return; // Success - exit retry loop
            }
            catch (Exception ex) when (IsTransientError(ex))
            {
                lastException = ex;

                if (_enableLogging && attempts < _maxAttempts)
                {
                    Console.Error.WriteLine(
                        string.Format("[Retry {0}/{1}] Transient error on '{2}': {3}. Retrying in {4}ms...",
                        attempts,
                        _maxAttempts - 1,
                        filePath,
                        ex.GetType().Name,
                        GetDelay(attempts).TotalMilliseconds));
                }

                if (attempts < _maxAttempts)
                {
                    await Task.Delay(GetDelay(attempts));
                }
            }
        }

        // All attempts failed - throw structured exception
        var message = string.Format("Failed after {0} attempts to perform file system operation on '{1}'. Last error: {2}",
            _maxAttempts, filePath, lastException?.Message ?? "unknown");
        throw new FileSystemException(message, lastException);
    }

    /// <summary>
    /// Executes a function with retry logic.
    /// </summary>
    /// <typeparam name="T">The return type of the function.</typeparam>
    /// <param name="func">The function to execute.</param>
    /// <param name="filePath">The file path associated with the operation (for error reporting).</param>
    /// <returns>A task representing the result of the function.</returns>
    public async Task<T> ExecuteAsync<T>(Func<Task<T>> func, string filePath)
    {
        ArgumentNullException.ThrowIfNull(func);
        ArgumentException.ThrowIfNullOrEmpty(filePath);

        var attempts = 0;
        Exception? lastException = null;

        while (attempts < _maxAttempts)
        {
            attempts++;

            try
            {
                return await func(); // Success - exit retry loop
            }
            catch (Exception ex) when (IsTransientError(ex))
            {
                lastException = ex;

                if (_enableLogging && attempts < _maxAttempts)
                {
                    Console.Error.WriteLine(
                        $"[Retry {0}/{1}] Transient error on '{2}': {3}. Retrying in {4}ms...",
                        attempts,
                        _maxAttempts - 1,
                        filePath,
                        ex.GetType().Name,
                        GetDelay(attempts).TotalMilliseconds);
                }

                if (attempts < _maxAttempts)
                {
                    await Task.Delay(GetDelay(attempts));
                }
            }
        }

        // All attempts failed - throw structured exception
        var message = string.Format("Failed after {0} attempts to perform file system operation on '{1}'. Last error: {2}",
            _maxAttempts, filePath, lastException?.Message ?? "unknown");
        throw new FileSystemException(message, lastException);
    }
}
