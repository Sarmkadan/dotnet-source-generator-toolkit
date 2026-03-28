// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetSourceGeneratorToolkit.Integration;

/// <summary>
/// Contract for sending webhook notifications about generation events.
/// Enables external systems to react to toolkit activity.
/// </summary>
public interface IWebhookService
{
    /// <summary>
    /// Register a webhook endpoint to receive generation events.
    /// </summary>
    /// <param name="name">Friendly name for this webhook</param>
    /// <param name="url">Absolute URL to POST events to</param>
    /// <param name="events">Types of events to subscribe to</param>
    /// <returns>Webhook ID for management</returns>
    Task<string> RegisterWebhookAsync(string name, string url, WebhookEventType[] events);

    /// <summary>
    /// Unregister a webhook from receiving events.
    /// </summary>
    /// <param name="webhookId">ID of webhook to unregister</param>
    /// <returns>Awaitable task</returns>
    Task UnregisterWebhookAsync(string webhookId);

    /// <summary>
    /// Send event to all registered webhooks that are subscribed to it.
    /// </summary>
    /// <param name="eventType">Type of event occurring</param>
    /// <param name="payload">Event data to send</param>
    /// <returns>Number of webhooks successfully notified</returns>
    Task<int> NotifyAsync(WebhookEventType eventType, object payload);

    /// <summary>
    /// Get all registered webhooks.
    /// </summary>
    /// <returns>List of webhook registrations</returns>
    Task<IEnumerable<WebhookRegistration>> GetWebhooksAsync();
}

/// <summary>
/// Types of events webhooks can subscribe to.
/// </summary>
public enum WebhookEventType
{
    GenerationStarted,
    GenerationCompleted,
    GenerationFailed,
    EntityAnalyzed,
    CodeGenerated,
}

/// <summary>
/// Webhook registration details.
/// </summary>
public class WebhookRegistration
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public WebhookEventType[] Events { get; set; } = Array.Empty<WebhookEventType>();
    public DateTime CreatedAt { get; set; }
    public DateTime? LastTriggeredAt { get; set; }
    public int FailureCount { get; set; }
}
