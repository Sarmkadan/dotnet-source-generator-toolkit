// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Logging;

namespace DotNetSourceGeneratorToolkit.Middleware;

/// <summary>
/// Middleware that provides centralized error handling and recovery strategies.
/// Catches exceptions and decides whether to retry, short-circuit, or propagate.
/// </summary>
public class ErrorHandlingMiddleware : IMiddleware
{
    private readonly ILogger<ErrorHandlingMiddleware> _logger;
    private const int MaxRetries = 3;
    private const int RetryDelayMs = 100;

    public ErrorHandlingMiddleware(ILogger<ErrorHandlingMiddleware> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task InvokeAsync(MiddlewareContext context, MiddlewareDelegate next)
    {
        var requestId = context.RequestId;
        var retryCount = 0;

        while (retryCount <= MaxRetries)
        {
            try
            {
                await next(context);
                return;
            }
            catch (IOException ioEx) when (retryCount < MaxRetries)
            {
                // Transient I/O errors may succeed on retry
                retryCount++;
                _logger.LogWarning(
                    "[{RequestId}] I/O error occurred, retry {Retry}/{Max}: {Message}",
                    requestId,
                    retryCount,
                    MaxRetries,
                    ioEx.Message);

                await Task.Delay(RetryDelayMs * retryCount);
            }
            catch (Exception ex)
            {
                // Non-recoverable errors are logged and propagated
                _logger.LogError(
                    "[{RequestId}] Unrecoverable error: {ExceptionType}: {Message}",
                    requestId,
                    ex.GetType().Name,
                    ex.Message);

                context.AddError($"Error: {ex.Message}");
                throw;
            }
        }

        var msg = "Maximum retries exceeded for I/O operation";
        _logger.LogError("[{RequestId}] {Message}", requestId, msg);
        context.AddError(msg);
    }
}
