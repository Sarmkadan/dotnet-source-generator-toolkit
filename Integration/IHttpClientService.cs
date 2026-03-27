// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetSourceGeneratorToolkit.Integration;

/// <summary>
/// Contract for making HTTP requests with retry and resilience policies.
/// Encapsulates HTTP communication for external API integrations.
/// </summary>
public interface IHttpClientService
{
    /// <summary>
    /// Send GET request and deserialize JSON response.
    /// </summary>
    /// <typeparam name="T">Type to deserialize response into</typeparam>
    /// <param name="url">Absolute URL to request</param>
    /// <returns>Deserialized response object</returns>
    Task<T?> GetAsync<T>(string url);

    /// <summary>
    /// Send POST request with JSON body and deserialize response.
    /// </summary>
    /// <typeparam name="TRequest">Type of request body</typeparam>
    /// <typeparam name="TResponse">Type to deserialize response into</typeparam>
    /// <param name="url">Absolute URL to request</param>
    /// <param name="body">Request body to serialize as JSON</param>
    /// <returns>Deserialized response object</returns>
    Task<TResponse?> PostAsync<TRequest, TResponse>(string url, TRequest body);

    /// <summary>
    /// Send PUT request with JSON body.
    /// </summary>
    /// <typeparam name="T">Type of request body</typeparam>
    /// <param name="url">Absolute URL to request</param>
    /// <param name="body">Request body to serialize as JSON</param>
    /// <returns>Awaitable task</returns>
    Task PutAsync<T>(string url, T body);

    /// <summary>
    /// Send DELETE request to URL.
    /// </summary>
    /// <param name="url">Absolute URL to request</param>
    /// <returns>Awaitable task</returns>
    Task DeleteAsync(string url);

    /// <summary>
    /// Send raw HTTP request and get response as string.
    /// </summary>
    /// <param name="method">HTTP method (GET, POST, etc)</param>
    /// <param name="url">Absolute URL to request</param>
    /// <param name="content">Optional request content</param>
    /// <returns>Response body as string</returns>
    Task<string> SendAsync(HttpMethod method, string url, HttpContent? content = null);
}
