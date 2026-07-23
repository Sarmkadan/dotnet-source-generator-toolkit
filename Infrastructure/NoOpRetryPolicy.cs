#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetSourceGeneratorToolkit.Infrastructure;

/// <summary>
/// A no-operation retry policy that executes actions immediately without retries.
/// Useful for testing scenarios where retry behavior should be disabled.
/// </summary>
public sealed class NoOpRetryPolicy : IRetryPolicy
{
    /// <summary>
    /// Determines whether a given exception represents a transient error that should be retried.
    /// </summary>
    /// <param name="exception">The exception to evaluate.</param>
    /// <returns>Always false - no errors are considered transient in this policy.</returns>
    public bool IsTransientError(Exception exception) => false;

    /// <summary>
    /// Gets the delay duration for a retry attempt.
    /// </summary>
    /// <param name="attempt">The retry attempt number (ignored in this implementation).</param>
    /// <returns>TimeSpan.Zero - no delay.</returns>
    public TimeSpan GetDelay(int attempt) => TimeSpan.Zero;

    /// <summary>
    /// Executes an action with retry logic (no retries in this implementation).
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <param name="filePath">The file path associated with the operation.</param>
    /// <returns>A task representing the operation.</returns>
    public Task ExecuteAsync(Func<Task> action, string filePath)
    {
        ArgumentNullException.ThrowIfNull(action);
        ArgumentException.ThrowIfNullOrEmpty(filePath);

        return action();
    }

    /// <summary>
    /// Executes a function with retry logic (no retries in this implementation).
    /// </summary>
    /// <typeparam name="T">The return type of the function.</typeparam>
    /// <param name="func">The function to execute.</param>
    /// <param name="filePath">The file path associated with the operation.</param>
    /// <returns>A task representing the result of the function.</returns>
    public Task<T> ExecuteAsync<T>(Func<Task<T>> func, string filePath)
    {
        ArgumentNullException.ThrowIfNull(func);
        ArgumentException.ThrowIfNullOrEmpty(filePath);

        return func();
    }
}
