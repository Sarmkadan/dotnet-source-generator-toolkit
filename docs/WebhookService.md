# WebhookService

The `WebhookService` provides asynchronous management and notification capabilities for webhook registrations, enabling external systems to be notified of events within the application. It supports registering, unregistering, and querying webhooks, as well as broadcasting notifications to registered endpoints.

## API

### `WebhookService`

The default constructor initializes a new instance of the `WebhookService` with default configuration.

### `RegisterWebhookAsync`

Registers a new webhook for the specified event type.

- **Parameters**
  - `EventType`: The type of event for which the webhook will be triggered.
  - `Url`: The endpoint URL to which the notification will be sent.
  - `Secret` *(optional)*: A secret key used to sign the payload for secure verification by the receiver.

- **Return Value**
  Returns a `Task<string>` representing the asynchronous operation. The task result contains a unique identifier for the registered webhook.

- **Exceptions**
  Throws `ArgumentNullException` if `EventType` or `Url` is `null`.
  Throws `ArgumentException` if `Url` is not a valid absolute URI.

### `UnregisterWebhookAsync`

Removes a previously registered webhook using its unique identifier.

- **Parameters**
  - `Id`: The unique identifier of the webhook to remove.

- **Return Value**
  Returns a `Task` representing the asynchronous operation.

- **Exceptions**
  Throws `ArgumentException` if `Id` is `null` or empty.

### `NotifyAsync`

Broadcasts a notification to all registered webhooks matching the specified event type.

- **Parameters**
  - `EventType`: The type of event being notified.
  - `Data`: The payload data to send to each registered webhook.

- **Return Value**
  Returns a `Task<int>` representing the asynchronous operation. The task result contains the number of webhooks successfully notified.

- **Exceptions**
  Throws `ArgumentNullException` if `EventType` is `null`.

### `GetWebhooksAsync`

Retrieves all currently registered webhooks.

- **Return Value**
  Returns a `Task<IEnumerable<WebhookRegistration>>` representing the asynchronous operation. The task result contains a collection of `WebhookRegistration` objects describing each registered webhook.

### `EventType`

Gets the event type associated with the current instance of the service. This property is read-only and reflects the event type used during construction or the last registration operation.

### `Timestamp`

Gets the timestamp associated with the current instance of the service. This property is read-only and reflects the time when the service instance was created or the last operation was performed.

### `Data`

Gets or sets the optional payload data associated with the current instance of the service. This property may be `null` if no data is set.

## Usage

### Registering and Notifying a Webhook
