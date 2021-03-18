# IWebhookService

The `IWebhookService` interface defines the core contract for representing a configured webhook within the `dotnet-source-generator-toolkit`. It encapsulates the necessary metadata required to identify, validate, and monitor the execution status of outgoing webhook notifications, providing a standardized structure for components that need to interact with webhook configurations.

## API

### `Id`
Gets the unique identifier associated with this webhook configuration.

### `Name`
Gets the human-readable display name for this webhook.

### `Url`
Gets the destination URL endpoint to which webhook events are transmitted. Implementations should ensure this is a well-formed URI.

### `Events`
Gets an array of `WebhookEventType` values indicating the specific events for which this webhook is subscribed.

### `CreatedAt`
Gets the `DateTime` representing when this webhook configuration was initially created or registered.

### `LastTriggeredAt`
Gets the `DateTime?` representing the timestamp of the most recent attempt to trigger this webhook. Returns `null` if the webhook has never been triggered.

### `FailureCount`
Gets the `int` representing the count of consecutive or total failures encountered during webhook delivery attempts.

## Usage

### Accessing Webhook Metadata
```csharp
public void LogWebhookInfo(IWebhookService webhook)
{
    Console.WriteLine($"Webhook: {webhook.Name} (ID: {webhook.Id})");
    Console.WriteLine($"Target URL: {webhook.Url}");
    Console.WriteLine($"Subscribed to: {string.Join(", ", webhook.Events)}");
}
```

### Handling Failed Webhooks
```csharp
public bool IsUnhealthy(IWebhookService webhook, int failureThreshold = 5)
{
    if (webhook.FailureCount >= failureThreshold)
    {
        return true;
    }
    
    // Check if it hasn't been triggered in over 24 hours while having failures
    if (webhook.LastTriggeredAt.HasValue && 
        (DateTime.UtcNow - webhook.LastTriggeredAt.Value).TotalHours > 24 && 
        webhook.FailureCount > 0)
    {
        return true;
    }
    
    return false;
}
```

## Notes

- **Thread Safety**: As an interface, the thread safety of `IWebhookService` depends entirely on the concrete implementation. If the implementing class updates properties like `FailureCount` or `LastTriggeredAt` while other threads read them, the implementation must provide appropriate synchronization (e.g., using `volatile` fields, `lock` statements, or atomic operations) to ensure consistency.
- **Nullability**: `LastTriggeredAt` is nullable (`DateTime?`). Consumers must always check `HasValue` before accessing the `Value` property to avoid `InvalidOperationException`.
- **Validation**: While this interface defines the schema for the data, it does not enforce runtime validation of the `Url` property. Users should validate the URL before attempting to use it for HTTP requests.
