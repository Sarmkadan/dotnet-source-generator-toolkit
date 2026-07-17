// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetSourceGeneratorToolkit.Integration;

/// <summary>
/// Provides extension methods for <see cref="WebhookService"/>.
/// </summary>
public static class WebhookServiceExtensions
{
    /// <summary>
    /// Registers a webhook with an enumerable collection of event types.
    /// </summary>
    /// <param name="service">The <see cref="WebhookService"/> instance.</param>
    /// <param name="name">The friendly name for the webhook.</param>
    /// <param name="url">The absolute URL to POST events to.</param>
    /// <param name="events">The collection of event types to subscribe to.</param>
    /// <returns>A task that represents the asynchronous operation, containing the webhook ID.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> or <paramref name="events"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> or <paramref name="url"/> is null or empty.</exception>
    public static async Task<string> RegisterWebhookAsync(
        this WebhookService service,
        string name,
        string url,
        IEnumerable<WebhookEventType> events)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(events);
        ArgumentException.ThrowIfNullOrEmpty(name);
        ArgumentException.ThrowIfNullOrEmpty(url);

        return await service.RegisterWebhookAsync(name, url, events.ToArray());
    }

    /// <summary>
    /// Retrieves a webhook registration by its ID.
    /// </summary>
    /// <param name="service">The <see cref="WebhookService"/> instance.</param>
    /// <param name="webhookId">The ID of the webhook to retrieve.</param>
    /// <returns>A task that represents the asynchronous operation, containing the <see cref="WebhookRegistration"/> if found; otherwise, null.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="webhookId"/> is null or empty.</exception>
    public static async Task<WebhookRegistration?> GetWebhookByIdAsync(
        this WebhookService service,
        string webhookId)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentException.ThrowIfNullOrEmpty(webhookId);

        var webhooks = await service.GetWebhooksAsync();
        return webhooks.FirstOrDefault(w => w.Id == webhookId);
    }

    /// <summary>
    /// Retrieves all webhook registrations that match the specified name.
    /// </summary>
    /// <param name="service">The <see cref="WebhookService"/> instance.</param>
    /// <param name="name">The name to filter by.</param>
    /// <returns>A task that represents the asynchronous operation, containing a list of matching <see cref="WebhookRegistration"/>s.
 /// The list is never null and contains zero or more elements.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> is null or empty.</exception>
    public static async Task<IReadOnlyList<WebhookRegistration>> GetWebhooksByNameAsync(
        this WebhookService service,
        string name)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentException.ThrowIfNullOrEmpty(name);

        var webhooks = await service.GetWebhooksAsync();
        return webhooks
 .Where(w => string.Equals(w.Name, name, StringComparison.OrdinalIgnoreCase))
 .ToList();
    }
}
