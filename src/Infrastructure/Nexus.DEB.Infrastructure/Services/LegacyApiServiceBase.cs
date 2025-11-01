using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace Nexus.DEB.Infrastructure.Services
{
    /// <summary>
    /// Base class for services that interact with legacy .NET Framework 4.x RESTful APIs.
    /// Provides common functionality for HTTP operations while ensuring authentication
    /// cookies are handled on a per-request basis to prevent cross-user contamination.
    /// </summary>
    /// <typeparam name="TService">The derived service type (for logging)</typeparam>
    public abstract class LegacyApiServiceBase<TService> where TService : class
    {
        protected readonly HttpClient HttpClient;
        protected readonly ILogger<TService> Logger;
        protected readonly JsonSerializerOptions JsonOptions;

        /// <summary>
        /// The name of the HttpClient configuration (e.g., "CisApi", "CbacApi")
        /// </summary>
        protected abstract string HttpClientName { get; }

        protected LegacyApiServiceBase(
            IHttpClientFactory httpClientFactory,
            ILogger<TService> logger)
        {
            HttpClient = httpClientFactory.CreateClient(HttpClientName);
            Logger = logger;

            // Configure JSON options to match legacy API response formats
            JsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        /// <summary>
        /// Creates an authenticated HTTP request with the provided authentication cookie.
        /// CRITICAL: This method creates a new HttpRequestMessage for each call to ensure
        /// that authentication cookies are NOT shared across different user requests.
        /// </summary>
        /// <param name="method">HTTP method</param>
        /// <param name="requestUri">Request URI (relative to base address)</param>
        /// <param name="authCookie">User-specific authentication cookie</param>
        /// <returns>A new HttpRequestMessage with the authentication cookie header</returns>
        /// <exception cref="InvalidOperationException">Thrown when authCookie is null or empty</exception>
        protected HttpRequestMessage CreateAuthenticatedRequest(
            HttpMethod method,
            string requestUri,
            string authCookie)
        {
            if (string.IsNullOrEmpty(authCookie))
            {
                Logger.LogError("Authentication cookie is missing for request to {RequestUri}", requestUri);
                throw new InvalidOperationException("Authentication cookie is required");
            }

            var request = new HttpRequestMessage(method, requestUri);

            // Add the Forms Authentication cookie to THIS REQUEST ONLY
            // This ensures the cookie is not shared across requests from different users
            request.Headers.Add("Cookie", authCookie);

            return request;
        }

        /// <summary>
        /// Sends an authenticated request and deserializes the JSON response.
        /// Handles common HTTP status codes (401, 403, 404) and provides consistent error handling.
        /// </summary>
        /// <typeparam name="TResponse">The expected response type</typeparam>
        /// <param name="method">HTTP method</param>
        /// <param name="requestUri">Request URI</param>
        /// <param name="authCookie">User-specific authentication cookie</param>
        /// <param name="operationName">Name of the operation (for logging)</param>
        /// <param name="content">Optional request content</param>
        /// <returns>The deserialized response, or null if the request was unsuccessful</returns>
        protected async Task<TResponse?> SendAuthenticatedRequestAsync<TResponse>(
            HttpMethod method,
            string requestUri,
            string authCookie,
            string operationName,
            HttpContent? content = null) where TResponse : class
        {
            try
            {
                Logger.LogInformation("Executing {OperationName}: {Method} {RequestUri}",
                    operationName, method, requestUri);

                var request = CreateAuthenticatedRequest(method, requestUri, authCookie);

                if (content != null)
                {
                    request.Content = content;
                }

                var response = await HttpClient.SendAsync(request);

                // Handle common HTTP status codes
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    Logger.LogWarning("{OperationName} failed: Unauthorized (401) for {RequestUri}",
                        operationName, requestUri);
                    return null;
                }

                if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    Logger.LogWarning("{OperationName} failed: Forbidden (403) for {RequestUri}",
                        operationName, requestUri);
                    return null;
                }

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    Logger.LogWarning("{OperationName} failed: Not Found (404) for {RequestUri}",
                        operationName, requestUri);
                    return null;
                }

                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<TResponse>(responseContent, JsonOptions);

                if (result == null)
                {
                    Logger.LogError("{OperationName}: Failed to deserialize response from {RequestUri}",
                        operationName, requestUri);
                    return null;
                }

                Logger.LogInformation("{OperationName} completed successfully", operationName);
                return result;
            }
            catch (HttpRequestException ex)
            {
                Logger.LogError(ex, "{OperationName}: HTTP request failed for {RequestUri}",
                    operationName, requestUri);
                throw;
            }
            catch (JsonException ex)
            {
                Logger.LogError(ex, "{OperationName}: Failed to parse response from {RequestUri}",
                    operationName, requestUri);
                throw new InvalidOperationException(
                    $"Failed to parse API response: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "{OperationName}: Unexpected error for {RequestUri}",
                    operationName, requestUri);
                throw;
            }
        }

        /// <summary>
        /// Sends an authenticated request and returns a boolean indicating success.
        /// Useful for validation endpoints that don't return data.
        /// </summary>
        /// <param name="method">HTTP method</param>
        /// <param name="requestUri">Request URI</param>
        /// <param name="authCookie">User-specific authentication cookie</param>
        /// <param name="operationName">Name of the operation (for logging)</param>
        /// <returns>True if successful (2xx status), false otherwise</returns>
        protected async Task<bool> SendAuthenticatedValidationRequestAsync(
            HttpMethod method,
            string requestUri,
            string authCookie,
            string operationName)
        {
            try
            {
                Logger.LogInformation("Executing {OperationName}: {Method} {RequestUri}",
                    operationName, method, requestUri);

                var request = CreateAuthenticatedRequest(method, requestUri, authCookie);
                var response = await HttpClient.SendAsync(request);

                if (response.StatusCode == HttpStatusCode.Unauthorized ||
                    response.StatusCode == HttpStatusCode.Forbidden)
                {
                    Logger.LogWarning("{OperationName} failed: {StatusCode} for {RequestUri}",
                        operationName, (int)response.StatusCode, requestUri);
                    return false;
                }

                response.EnsureSuccessStatusCode();

                Logger.LogInformation("{OperationName} completed successfully", operationName);
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "{OperationName}: Error for {RequestUri}",
                    operationName, requestUri);
                throw;
            }
        }

        /// <summary>
        /// Sends an unauthenticated request (for endpoints like login that don't require cookies).
        /// </summary>
        /// <typeparam name="TResponse">The expected response type</typeparam>
        /// <param name="method">HTTP method</param>
        /// <param name="requestUri">Request URI</param>
        /// <param name="operationName">Name of the operation (for logging)</param>
        /// <param name="content">Optional request content</param>
        /// <returns>The deserialized response, or null if the request was unsuccessful</returns>
        protected async Task<TResponse?> SendUnauthenticatedRequestAsync<TResponse>(
            HttpMethod method,
            string requestUri,
            string operationName,
            HttpContent? content = null) where TResponse : class
        {
            try
            {
                Logger.LogInformation("Executing {OperationName}: {Method} {RequestUri}",
                    operationName, method, requestUri);

                var request = new HttpRequestMessage(method, requestUri);

                if (content != null)
                {
                    request.Content = content;
                }

                var response = await HttpClient.SendAsync(request);

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    Logger.LogWarning("{OperationName} failed: Unauthorized (401) for {RequestUri}",
                        operationName, requestUri);
                    return null;
                }

                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<TResponse>(responseContent, JsonOptions);

                if (result == null)
                {
                    Logger.LogError("{OperationName}: Failed to deserialize response from {RequestUri}",
                        operationName, requestUri);
                    return null;
                }

                Logger.LogInformation("{OperationName} completed successfully", operationName);
                return result;
            }
            catch (HttpRequestException ex)
            {
                Logger.LogError(ex, "{OperationName}: HTTP request failed for {RequestUri}",
                    operationName, requestUri);
                throw new InvalidOperationException(
                    $"Failed to communicate with API: {ex.Message}", ex);
            }
            catch (JsonException ex)
            {
                Logger.LogError(ex, "{OperationName}: Failed to parse response from {RequestUri}",
                    operationName, requestUri);
                throw new InvalidOperationException(
                    $"Failed to parse API response: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "{OperationName}: Unexpected error for {RequestUri}",
                    operationName, requestUri);
                throw;
            }
        }
    }
}
