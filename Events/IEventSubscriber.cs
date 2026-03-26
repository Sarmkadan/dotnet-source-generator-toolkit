// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetSourceGeneratorToolkit.Events;

/// <summary>
/// Contract for handling domain events published by the event aggregator.
/// Each handler is specialized to one event type for type safety.
/// </summary>
/// <typeparam name="TEvent">Type of event this handler processes</typeparam>
public interface IEventHandler<TEvent> where TEvent : IDomainEvent
{
    /// <summary>
    /// Handle an event and perform necessary side effects.
    /// </summary>
    /// <param name="event">Event to handle</param>
    /// <returns>Awaitable task representing handler execution</returns>
    Task HandleAsync(TEvent @event);
}
