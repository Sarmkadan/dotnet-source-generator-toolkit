// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace DotNetSourceGeneratorToolkit.Integration;

/// <summary>
/// Manages webhook registrations and asynchronously notifies subscribers of events.
/// In-memory storage suitable for single-session use; extend with persistent storage for production.
/// </summary>
public class WebhookService : IWebhookService
{
    private readonly Dictionary<string, WebhookRegistration> _webhooks = new();
    private readonly IHttpClientService _httpClient;
    private readonly ILogger<WebhookService> _logger;

    public WebhookService(IHttpClientService httpClient, ILogger<WebhookService> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<string> RegisterWebhookAsync(string name, string url, WebhookEventType[] events)
    {
        var id = Guid.NewGuid().ToString();
        var webhook = new WebhookRegistration
        {
            Id = id,
            Name = name,
            Url = url,
            Events = events,
            CreatedAt = DateTime.UtcNow,
        };

        _webhooks[id] = webhook;

        _logger.LogInformation(
            "Webhook registered: {Name} ({Id}) for events: {Events}",
            name,
            id,
            string.Join(", ", events));

        return id;
    }

    public async Task UnregisterWebhookAsync(string webhookId)
    {
        if (_webhooks.Remove(webhookId))
        {
            _logger.LogInformation("Webhook unregistered: {WebhookId}", webhookId);
        }
        else
        {
            _logger.LogWarning("Webhook not found: {WebhookId}", webhookId);
        }

        await Task.CompletedTask;
    }

    public async Task<int> NotifyAsync(WebhookEventType eventType, object payload)
    {
        var applicableWebhooks = _webhooks.Values
            .Where(w => w.Events.Contains(eventType))
            .ToList();

        if (applicableWebhooks.Count == 0)
        {
            _logger.LogDebug("No webhooks registered for event: {EventType}", eventType);
            return 0;
        }

        var notifyTasks = applicableWebhooks.Select(webhook => NotifyWebhookAsync(webhook, eventType, payload));
        var results = await Task.WhenAll(notifyTasks);

        return results.Count(success => success);
    }

    public async Task<IEnumerable<WebhookRegistration>> GetWebhooksAsync()
    {
        return await Task.FromResult(_webhooks.Values.ToList());
    }

    private async Task<bool> NotifyWebhookAsync(WebhookRegistration webhook, WebhookEventType eventType, object payload)
    {
        try
        {
            var envelope = new WebhookPayload
            {
                EventType = eventType,
                Timestamp = DateTime.UtcNow,
                Data = payload,
            };

            await _httpClient.PostAsync<WebhookPayload, object>(webhook.Url, envelope);

            webhook.LastTriggeredAt = DateTime.UtcNow;
            webhook.FailureCount = 0;

            _logger.LogInformation(
                "Webhook {Name} ({Id}) notified for event {EventType}",
                webhook.Name,
                webhook.Id,
                eventType);

            return true;
        }
        catch (Exception ex)
        {
            webhook.FailureCount++;

            _logger.LogWarning(
                ex,
                "Webhook {Name} ({Id}) notification failed for event {EventType}. Failures: {Count}",
                webhook.Name,
                webhook.Id,
                eventType,
                webhook.FailureCount);

            return false;
        }
    }

    public class WebhookPayload
    {
        [JsonPropertyName("eventType")]
        public WebhookEventType EventType { get; set; }

        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonPropertyName("data")]
        public object? Data { get; set; }
    }
}
