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
            "[{RequestId}] EVENT: Generation started for project '{ProjectName}' at {Timestamp}",
            @event.RequestId,
            @event.ProjectName,
            @event.Timestamp);
            
        return Task.CompletedTask;
    }

    public Task HandleAsync(GenerationCompletedEvent @event)
    {
        _logger.LogInformation(
            "[{RequestId}] EVENT: Generation completed for project '{ProjectName}'. Status: {Status}, Duration: {DurationMs}ms, Warnings: {Warnings}, Errors: {Errors}",
            @event.RequestId,
            @event.ProjectName,
            @event.Status,
            @event.DurationMs,
            @event.WarningCount,
            @event.ErrorCount);
            
        return Task.CompletedTask;
    }
}
