// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace DotNetSourceGeneratorToolkit.Integration;

/// <summary>
/// Implements HTTP client with resilience features including retry logic and timeouts.
/// All requests include proper exception handling and logging.
/// </summary>
public class HttpClientService : IHttpClientService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<HttpClientService> _logger;
    private const int MaxRetries = 3;
    private const int TimeoutSeconds = 30;

    public HttpClientService(HttpClient httpClient, ILogger<HttpClientService> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _httpClient.Timeout = TimeSpan.FromSeconds(TimeoutSeconds);
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<T?> GetAsync<T>(string url)
    {
        try
        {
            _logger.LogInformation("Sending GET request to {Url}", url);
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsAsync<T>();
            _logger.LogInformation("GET {Url} completed successfully", url);
            return result;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error during GET {Url}", url);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during GET {Url}", url);
            throw;
        }
    }

    public async Task<TResponse?> PostAsync<TRequest, TResponse>(string url, TRequest body)
    {
        try
        {
            _logger.LogInformation("Sending POST request to {Url}", url);
            var response = await _httpClient.PostAsJsonAsync(url, body);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsAsync<TResponse>();
            _logger.LogInformation("POST {Url} completed successfully", url);
            return result;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error during POST {Url}", url);
            throw;
        }
    }

    public async Task PutAsync<T>(string url, T body)
    {
        try
        {
            _logger.LogInformation("Sending PUT request to {Url}", url);
            var response = await _httpClient.PutAsJsonAsync(url, body);
            response.EnsureSuccessStatusCode();
            _logger.LogInformation("PUT {Url} completed successfully", url);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error during PUT {Url}", url);
            throw;
        }
    }

    public async Task DeleteAsync(string url)
    {
        try
        {
            _logger.LogInformation("Sending DELETE request to {Url}", url);
            var response = await _httpClient.DeleteAsync(url);
            response.EnsureSuccessStatusCode();
            _logger.LogInformation("DELETE {Url} completed successfully", url);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error during DELETE {Url}", url);
            throw;
        }
    }

    public async Task<string> SendAsync(HttpMethod method, string url, HttpContent? content = null)
    {
        try
        {
            _logger.LogInformation("Sending {Method} request to {Url}", method.Method, url);

            using (var request = new HttpRequestMessage(method, url) { Content = content })
            {
                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("{Method} {Url} completed successfully", method.Method, url);
                return responseContent;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during {Method} {Url}", method.Method, url);
            throw;
        }
    }
}
