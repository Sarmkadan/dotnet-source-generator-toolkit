// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Logging;

namespace DotNetSourceGeneratorToolkit.Middleware;

/// <summary>
/// Middleware that logs pipeline execution details for debugging and auditing.
/// Records request start, completion, and any errors encountered.
/// </summary>
public class LoggingMiddleware : IMiddleware
{
    private readonly ILogger<LoggingMiddleware> _logger;

    public LoggingMiddleware(ILogger<LoggingMiddleware> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task InvokeAsync(MiddlewareContext context, MiddlewareDelegate next)
    {
        var requestId = context.RequestId;
        var projectPath = context.CliOptions?.ProjectPath ?? "unknown";

        _logger.LogInformation(
            "[{RequestId}] Pipeline started for project: {ProjectPath}",
            requestId,
            projectPath);

        var startTime = DateTime.UtcNow;

        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            var elapsed = DateTime.UtcNow - startTime;

            _logger.LogError(
                ex,
                "[{RequestId}] Pipeline failed after {Elapsed:F2}ms: {Message}",
                requestId,
                elapsed.TotalMilliseconds,
                ex.Message);

            context.AddError($"Pipeline error: {ex.Message}");
            throw;
        }

        var duration = DateTime.UtcNow - startTime;

        var errorCount = context.Errors.Count;
        var resultCount = context.GenerationResults.Count;

        _logger.LogInformation(
            "[{RequestId}] Pipeline completed in {Elapsed:F2}ms - {Results} results, {Errors} errors",
            requestId,
            duration.TotalMilliseconds,
            resultCount,
            errorCount);

        if (errorCount > 0 && context.CliOptions?.Verbose == true)
        {
            foreach (var error in context.Errors)
            {
                _logger.LogWarning("[{RequestId}] Error: {ErrorMessage}", requestId, error);
            }
        }
    }
}
