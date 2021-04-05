using System;
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
        public static async Task<T?> GetOrDefaultAsync<T>(this HttpClientService service, string url, T? defaultValue = default)
        {
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
        public static async Task<TResponse?> PostJsonAsync<TRequest, TResponse>(this HttpClientService service, string url, TRequest body)
        {
            // The underlying service already handles JSON serialization/deserialization,
            // so we simply forward the call.
            return await service.PostAsync<TRequest, TResponse>(url, body).ConfigureAwait(false);
        }

        /// <summary>
        /// Sends a request with an optional JSON payload using the specified HTTP method.
        /// Returns the raw response string.
        /// </summary>
        public static async Task<string> SendJsonAsync(this HttpClientService service, HttpMethod method, string url, object? payload = null)
        {
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
        public static async Task<bool> DeleteIfExistsAsync(this HttpClientService service, string url)
        {
            try
            {
                await service.DeleteAsync(url).ConfigureAwait(false);
                return true;
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return false;
            }
        }
    }
}
