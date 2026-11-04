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

    /// <summary>
    /// Initializes a new instance of the <see cref="LoggingEventHandler"/> class.
    /// </summary>
    /// <param name="logger">The logger instance to use for logging events.</param>
    /// <exception cref="ArgumentNullException">Thrown when the logger is null.</exception>
    public LoggingEventHandler(ILogger<LoggingEventHandler> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Handles the <see cref="GenerationStartedEvent"/> by logging the event details.
    /// </summary>
    /// <param name="event">The generation started event to handle.</param>
    /// <returns>A completed task.</returns>
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

    /// <summary>
    /// Handles the <see cref="GenerationCompletedEvent"/> by logging the event details.
    /// </summary>
    /// <param name="event">The generation completed event to handle.</param>
    /// <returns>A completed task.</returns>
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
