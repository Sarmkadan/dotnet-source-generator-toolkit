// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetSourceGeneratorToolkit.Events;

/// <summary>
/// Contract for publishing domain events throughout the generation pipeline.
/// Decouples event producers from consumers via publish-subscribe pattern.
/// </summary>
public interface IEventPublisher
{
    /// <summary>
    /// Publish an event to all subscribed handlers.
    /// </summary>
    /// <typeparam name="TEvent">Type of event being published</typeparam>
    /// <param name="event">Event instance to publish</param>
    /// <returns>Awaitable task that completes when all handlers finish</returns>
    Task PublishAsync<TEvent>(TEvent @event) where TEvent : IDomainEvent;
}

/// <summary>
/// Base interface for all domain events.
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// Unique identifier for this event instance.
    /// </summary>
    string EventId { get; }

    /// <summary>
    /// UTC timestamp when event was created.
    /// </summary>
    DateTime OccurredAt { get; }

    /// <summary>
    /// Request ID for tracing correlation.
    /// </summary>
    string RequestId { get; }
}
