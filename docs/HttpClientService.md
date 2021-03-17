# HttpClientService

The `HttpClientService` class provides a streamlined, asynchronous wrapper around `System.Net.Http.HttpClient` to facilitate common RESTful HTTP operations. It encapsulates JSON serialization and deserialization, simplifying interactions with APIs while providing a consistent interface for GET, POST, PUT, DELETE, and generic request processing.

## API

### HttpClientService()
Initializes a new instance of the `HttpClientService` class, preparing it for HTTP requests.

### async Task<T?> GetAsync<T>(string requestUri)
Sends an HTTP GET request to the specified URI.
*   **Parameters:** `requestUri` (The URL to call).
*   **Returns:** A deserialized instance of type `T`, or `null` if the response content is empty or the request fails.
*   **Throws:** `HttpRequestException` for network or server-side issues.

### async Task<TResponse?> PostAsync<TRequest, TResponse>(string requestUri, TRequest content)
Sends an HTTP POST request to the specified URI with the provided body.
*   **Parameters:** `requestUri` (The URL to call), `content` (The object to serialize as JSON for the body).
*   **Returns:** A deserialized instance of type `TResponse`, or `null` if the response content is empty.
*   **Throws:** `HttpRequestException` for network or server-side issues.

### async Task PutAsync<T>(string requestUri, T content)
Sends an HTTP PUT request to the specified URI with the provided body.
*   **Parameters:** `requestUri` (The URL to call), `content` (The object to serialize as JSON for the body).
*   **Throws:** `HttpRequestException` for network or server-side issues.

### async Task DeleteAsync(string requestUri)
Sends an HTTP DELETE request to the specified URI.
*   **Parameters:** `requestUri` (The URL to call).
*   **Throws:** `HttpRequestException` for network or server-side issues.

### async Task<string> SendAsync(HttpRequestMessage request)
Sends a custom `HttpRequestMessage` and returns the response body as a raw string.
*   **Parameters:** `request` (The fully constructed HTTP request).
*   **Returns:** The response content as a string.
*   **Throws:** `HttpRequestException` for network or server-side issues.

## Usage

```csharp
// Example 1: Basic GET operation
var service = new HttpClientService();
var user = await service.GetAsync<User>("https://api.example.com/users/1");
if (user != null)
{
    Console.WriteLine($"User: {user.Name}");
}
```

```csharp
// Example 2: POST operation with request/response objects
var service = new HttpClientService();
var newProduct = new Product { Name = "New Widget", Price = 19.99 };
var createdProduct = await service.PostAsync<Product, Product>("https://api.example.com/products", newProduct);
```

## Notes

- **Error Handling**: Methods throw `HttpRequestException` when the server returns a non-success status code or when a network error occurs. Consumers should implement appropriate `try-catch` blocks.
- **Serialization**: JSON serialization assumes default conventions.
- **Thread Safety**: This service is designed to be thread-safe when consuming a single `HttpClient` instance across multiple requests. It is recommended to register this service as a Singleton in dependency injection containers to reuse the underlying `HttpClient` handler effectively.
- **Response Types**: Generic methods expect the response body to be valid JSON convertible to the target type `T`. If the API returns raw text or other formats, `SendAsync` should be used instead.
