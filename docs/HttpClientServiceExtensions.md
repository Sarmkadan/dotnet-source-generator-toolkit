# HttpClientServiceExtensions

Provides convenience extension methods for `HttpClient` that simplify common HTTP operations with built-in JSON serialization and deserialization. These methods reduce boilerplate code for sending requests, handling responses, and managing transient HTTP errors by returning sensible defaults or throwing only on unrecoverable failures.

## API

### GetOrDefaultAsync\<T\>

```csharp
public static async Task<T?> GetOrDefaultAsync<T>(this HttpClient client, string requestUri)
```

Performs an HTTP GET request and deserializes the response body as JSON into an instance of `T`. If the request fails due to a non-success status code or any exception during the operation, the method returns `default(T)` (typically `null` for reference types) rather than throwing.

**Parameters:**
- `client` — The `HttpClient` instance on which the extension method operates.
- `requestUri` — The URI the GET request is sent to.

**Return value:**
A `Task<T?>` that resolves to the deserialized object of type `T` on success, or `default(T?)` when the response indicates failure or an exception occurs.

**Throws:**
- `ArgumentNullException` — if `client` or `requestUri` is `null`.

---

### PostJsonAsync\<TRequest, TResponse\>

```csharp
public static async Task<TResponse?> PostJsonAsync<TRequest, TResponse>(
    this HttpClient client, string requestUri, TRequest content)
```

Serializes `content` as JSON and sends it in an HTTP POST request. On a successful response, the response body is deserialized as `TResponse`. If the response status code indicates failure or any exception occurs, the method returns `default(TResponse)`.

**Parameters:**
- `client` — The `HttpClient` instance.
- `requestUri` — The target URI for the POST request.
- `content` — The request payload to be serialized as JSON.

**Return value:**
A `Task<TResponse?>` that resolves to the deserialized response object on success, or `default(TResponse?)` on failure.

**Throws:**
- `ArgumentNullException` — if `client` or `requestUri` is `null`.

---

### SendJsonAsync

```csharp
public static async Task<string> SendJsonAsync(
    this HttpClient client, HttpMethod method, string requestUri, object? content = null)
```

Serializes an optional payload as JSON and sends it using the specified HTTP method. Returns the raw response body as a string on success. If the response status code is not a success code or an exception occurs, an empty string (`""`) is returned.

**Parameters:**
- `client` — The `HttpClient` instance.
- `method` — The `HttpMethod` to use for the request (e.g., `HttpMethod.Put`, `HttpMethod.Patch`).
- `requestUri` — The target URI.
- `content` — Optional object to serialize as the JSON request body. Pass `null` to send no body.

**Return value:**
A `Task<string>` that resolves to the response body string on success, or an empty string on failure.

**Throws:**
- `ArgumentNullException` — if `client`, `method`, or `requestUri` is `null`.

---

### DeleteIfExistsAsync

```csharp
public static async Task<bool> DeleteIfExistsAsync(this HttpClient client, string requestUri)
```

Sends an HTTP DELETE request to the specified URI. Returns `true` if the response status code indicates success (2xx), and `false` if the status code is 404 (Not Found) or any other non-success code, or if an exception occurs during the request.

**Parameters:**
- `client` — The `HttpClient` instance.
- `requestUri` — The URI identifying the resource to delete.

**Return value:**
A `Task<bool>` that resolves to `true` when the resource was successfully deleted, `false` otherwise.

**Throws:**
- `ArgumentNullException` — if `client` or `requestUri` is `null`.

## Usage

### Example 1: Fetching a resource with a fallback

```csharp
using var client = new HttpClient { BaseAddress = new Uri("https://api.example.com/") };

// Attempt to retrieve a user profile; returns null if the user doesn't exist
// or the service is unavailable.
UserProfile? profile = await client.GetOrDefaultAsync<UserProfile>("users/42");

if (profile is not null)
{
    Console.WriteLine($"Found user: {profile.DisplayName}");
}
else
{
    Console.WriteLine("User not found or service unavailable — using default profile.");
}
```

### Example 2: Conditional delete with JSON POST fallback

```csharp
using var client = new HttpClient { BaseAddress = new Uri("https://api.example.com/") };

// Try to delete a resource; proceed only if it actually existed.
bool deleted = await client.DeleteIfExistsAsync("orders/789");

if (deleted)
{
    Console.WriteLine("Order successfully removed.");
}
else
{
    // Resource didn't exist — create it via JSON POST.
    var newOrder = new { Item = "Widget", Quantity = 5 };
    OrderConfirmation? confirmation = await client.PostJsonAsync<object, OrderConfirmation>(
        "orders", newOrder);

    Console.WriteLine(confirmation is not null
        ? $"Created order {confirmation.OrderId}"
        : "Failed to create order.");
}
```

## Notes

- **Error handling strategy:** All methods suppress exceptions from the underlying HTTP call (e.g., `HttpRequestException`, `TaskCanceledException`, serialization errors) and return a neutral default value. The only exceptions that propagate are precondition failures (`ArgumentNullException`). Callers that need detailed error information should use standard `HttpClient` methods directly.
- **Status code treatment:** `GetOrDefaultAsync`, `PostJsonAsync`, and `SendJsonAsync` treat any non-2xx status code as a failure and return their respective default. `DeleteIfExistsAsync` distinguishes 404 from other failures but still returns `false` for both — the distinction is swallowed; the return value solely indicates whether the resource was confirmed deleted.
- **Serialization defaults:** The methods rely on a default `System.Text.Json` serializer configuration (case-sensitive property matching, no custom converters unless configured globally). Ensure request and response types are compatible with these defaults.
- **Thread safety:** All methods are stateless extensions that operate on the provided `HttpClient` instance. `HttpClient` itself is thread-safe, so these methods can be called concurrently from multiple threads as long as the same `HttpClient` instance is used according to its documented threading guidelines.
- **Empty response bodies:** When a successful response has no body (e.g., 204 No Content), `GetOrDefaultAsync` and `PostJsonAsync` will return `default(T)` because deserialization of empty content yields the default value. `SendJsonAsync` returns an empty string in such cases.
- **Null content in SendJsonAsync:** When `content` is `null`, no request body is sent. This is suitable for methods like GET or DELETE when called through this generic method, though dedicated methods (`GetOrDefaultAsync`, `DeleteIfExistsAsync`) are preferred for clarity.
