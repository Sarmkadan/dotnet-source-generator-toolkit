#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetSourceGeneratorToolkit.Infrastructure;

/// <summary>
/// Defines a retry policy for handling transient failures in file system operations.
/// </summary>
public interface IRetryPolicy
{
    /// <summary>
    /// Determines whether a given exception represents a transient error that should be retried.
    /// </summary>
    /// <param name="exception">The exception to evaluate.</param>
    /// <returns>True if the exception is transient and should be retried; otherwise, false.</returns>
    bool IsTransientError(Exception exception);

    /// <summary>
    /// Gets the delay duration for a retry attempt.
    /// </summary>
    /// <param name="attempt">The retry attempt number (1-based).</param>
    /// <returns>The delay duration before the next retry attempt.</returns>
    TimeSpan GetDelay(int attempt);

    /// <summary>
    /// Executes an action with retry logic.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <param name="filePath">The file path associated with the operation (for error reporting).</param>
    /// <returns>A task representing the operation.</returns>
    Task ExecuteAsync(Func<Task> action, string filePath);

    /// <summary>
    /// Executes a function with retry logic.
    /// </summary>
    /// <typeparam name="T">The return type of the function.</typeparam>
    /// <param name="func">The function to execute.</param>
    /// <param name="filePath">The file path associated with the operation (for error reporting).</param>
    /// <returns>A task representing the result of the function.</returns>
    Task<T> ExecuteAsync<T>(Func<Task<T>> func, string filePath);
}
