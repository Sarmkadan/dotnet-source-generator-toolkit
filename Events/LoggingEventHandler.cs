#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Logging;

namespace DotNetSourceGeneratorToolkit.Events;

/// <summary>
/// A generic event handler that logs when specific domain events are published.
/// </summary>
public sealed class LoggingEventHandler : 
    IEventHandler<GenerationStartedEvent>, 
    IEventHandler<GenerationCompletedEvent>
{
    private readonly ILogger<LoggingEventHandler> _logger;

    public LoggingEventHandler(ILogger<LoggingEventHandler> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task HandleAsync(GenerationStartedEvent @event)
    {
        _logger.LogInformation(
            "[{RequestId}] EVENT: Generation started for project '{ProjectPath}' at {Timestamp} ({EntityCount} entities)",
            @event.RequestId,
            @event.ProjectPath,
            @event.OccurredAt,
            @event.EntityCount);

        return Task.CompletedTask;
    }

    public Task HandleAsync(GenerationCompletedEvent @event)
    {
        _logger.LogInformation(
            "[{RequestId}] EVENT: Generation completed. Success: {IsSuccessful}, Duration: {DurationMs}ms, FilesGenerated: {FilesGenerated}, Errors: {ErrorCount}",
            @event.RequestId,
            @event.IsSuccessful,
            @event.ExecutionTimeMs,
            @event.FilesGenerated,
            @event.Errors.Count);

        return Task.CompletedTask;
    }
}
