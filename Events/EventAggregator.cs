// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DotNetSourceGeneratorToolkit.Events;

/// <summary>
/// Implements pub-sub event pattern using service provider for handler discovery.
/// Executes all matching handlers sequentially and logs event flow.
/// </summary>
public class EventAggregator : IEventPublisher
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EventAggregator> _logger;

    public EventAggregator(IServiceProvider serviceProvider, ILogger<EventAggregator> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task PublishAsync<TEvent>(TEvent @event) where TEvent : IDomainEvent
    {
        if (@event == null)
            throw new ArgumentNullException(nameof(@event));

        var eventType = typeof(TEvent);
        var handlerType = typeof(IEventHandler<>).MakeGenericType(eventType);

        _logger.LogInformation(
            "[{RequestId}] Publishing event {EventType} (ID: {EventId})",
            @event.RequestId,
            eventType.Name,
            @event.EventId);

        try
        {
            // Resolve all handlers for this event type
            var handlersMethod = typeof(ServiceProviderServiceExtensions)
                .GetMethod("GetServices")!
                .MakeGenericMethod(handlerType);

            var handlers = handlersMethod.Invoke(null, new object[] { _serviceProvider }) as System.Collections.IEnumerable;

            if (handlers == null)
            {
                _logger.LogWarning(
                    "[{RequestId}] No handlers found for event {EventType}",
                    @event.RequestId,
                    eventType.Name);
                return;
            }

            // Execute all handlers
            var handlerCount = 0;
            foreach (var handler in handlers)
            {
                var handleMethod = handlerType.GetMethod("HandleAsync")
                    ?? throw new InvalidOperationException($"Handler missing HandleAsync method");

                try
                {
                    await (Task)handleMethod.Invoke(handler, new object[] { @event })!;
                    handlerCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "[{RequestId}] Handler {HandlerType} failed for event {EventType}",
                        @event.RequestId,
                        handler?.GetType().Name,
                        eventType.Name);
                }
            }

            _logger.LogInformation(
                "[{RequestId}] Event {EventType} published to {HandlerCount} handler(s)",
                @event.RequestId,
                eventType.Name,
                handlerCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "[{RequestId}] Error publishing event {EventType}",
                @event.RequestId,
                eventType.Name);
            throw;
        }
    }
}
