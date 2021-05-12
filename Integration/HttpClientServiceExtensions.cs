using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DotNetSourceGeneratorToolkit.Integration
{
    /// <summary>
    /// Extension methods that add convenient helpers for <see cref="HttpClientService"/>.
    /// </summary>
    public static class HttpClientServiceExtensions
    {
        /// <summary>
        /// Executes a GET request and returns the deserialized result, or a supplied default value
        /// if the request fails for any reason.
        /// </summary>
        /// <typeparam name="T">The type to deserialize the response into.</typeparam>
        /// <param name="service">The HTTP client service instance.</param>
        /// <param name="url">The URL to request.</param>
        /// <param name="defaultValue">The value to return if the request fails.</param>
        /// <returns>The deserialized response or default value on failure.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="service"/> or <paramref name="url"/> is <see langword="null"/>.</exception>
        public static async Task<T?> GetOrDefaultAsync<T>(this HttpClientService service, string url, T? defaultValue = default)
        {
            ArgumentNullException.ThrowIfNull(service);
            ArgumentNullException.ThrowIfNull(url);

            try
            {
                return await service.GetAsync<T>(url).ConfigureAwait(false);
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Sends a POST request with a JSON payload and returns the deserialized response.
        /// </summary>
        /// <typeparam name="TRequest">The type of the request payload.</typeparam>
        /// <typeparam name="TResponse">The type to deserialize the response into.</typeparam>
        /// <param name="service">The HTTP client service instance.</param>
        /// <param name="url">The URL to request.</param>
        /// <param name="body">The request payload to serialize as JSON.</param>
        /// <returns>The deserialized response.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="service"/>, <paramref name="url"/>, or <paramref name="body"/> is <see langword="null"/>.</exception>
        public static async Task<TResponse?> PostJsonAsync<TRequest, TResponse>(this HttpClientService service, string url, TRequest body)
        {
            ArgumentNullException.ThrowIfNull(service);
            ArgumentNullException.ThrowIfNull(url);
            ArgumentNullException.ThrowIfNull(body);

            return await service.PostAsync<TRequest, TResponse>(url, body).ConfigureAwait(false);
        }

        /// <summary>
        /// Sends a request with an optional JSON payload using the specified HTTP method.
        /// Returns the raw response string.
        /// </summary>
        /// <param name="service">The HTTP client service instance.</param>
        /// <param name="method">The HTTP method to use.</param>
        /// <param name="url">The URL to request.</param>
        /// <param name="payload">Optional request payload to serialize as JSON.</param>
        /// <returns>The raw response string.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="service"/>, <paramref name="method"/>, or <paramref name="url"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="url"/> is <see cref="string.Empty"/>.</exception>
        public static async Task<string> SendJsonAsync(this HttpClientService service, HttpMethod method, string url, object? payload = null)
        {
            ArgumentNullException.ThrowIfNull(service);
            ArgumentNullException.ThrowIfNull(method);
            ArgumentException.ThrowIfNullOrEmpty(url);

            HttpContent? content = null;

            if (payload != null)
            {
                string json = JsonSerializer.Serialize(payload);
                content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            return await service.SendAsync(method, url, content).ConfigureAwait(false);
        }

        /// <summary>
        /// Attempts to delete a resource; returns <c>true</c> if the delete succeeded,
        /// or <c>false</c> if the resource was not found (HTTP 404). Other exceptions are re‑thrown.
        /// </summary>
        /// <param name="service">The HTTP client service instance.</param>
        /// <param name="url">The URL of the resource to delete.</param>
        /// <returns><c>true</c> if the resource was deleted; <c>false</c> if it didn't exist.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="service"/> or <paramref name="url"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="url"/> is <see cref="string.Empty"/>.</exception>
        public static async Task<bool> DeleteIfExistsAsync(this HttpClientService service, string url)
        {
            ArgumentNullException.ThrowIfNull(service);
            ArgumentException.ThrowIfNullOrEmpty(url);

            try
            {
                await service.DeleteAsync(url).ConfigureAwait(false);
                return true;
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return false;
            }
        }
    }
}
