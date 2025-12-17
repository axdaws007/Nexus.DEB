using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace Nexus.DEB.Infrastructure.Services
{
    /// <summary>
    /// Base class for services that interact with legacy .NET Framework 4.x RESTful APIs.
    /// Provides common functionality for HTTP operations while ensuring authentication
    /// cookies are handled on a per-request basis to prevent cross-user contamination.
    /// 
    /// SECURITY: Authentication cookies are retrieved from HttpContext, which is request-scoped.
    /// This ensures cookies are NEVER shared across different user requests.
    /// </summary>
    /// <typeparam name="TService">The derived service type (for logging)</typeparam>
    public abstract class LegacyApiServiceBase<TService> where TService : class
    {
        protected readonly HttpClient HttpClient;
        protected readonly ILogger<TService> Logger;
        protected readonly JsonSerializerOptions JsonOptions;
        protected readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _authCookieName;

        /// <summary>
        /// The name of the HttpClient configuration (e.g., "CisApi", "CbacApi", "WorkflowApi")
        /// </summary>
        protected abstract string HttpClientName { get; }

        protected LegacyApiServiceBase(
            IHttpClientFactory httpClientFactory,
            ILogger<TService> logger,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration)
        {
            HttpClient = httpClientFactory.CreateClient(HttpClientName);
            Logger = logger;
            _httpContextAccessor = httpContextAccessor;

            _authCookieName = configuration["Authentication:CookieName"]
                ?? throw new InvalidOperationException("Authentication:CookieName is not configured");

            // Configure JSON options to match legacy API response formats
            JsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        /// <summary>
        /// Gets the authentication cookie from the current HTTP context.
        /// 
        /// SECURITY: This method retrieves the Forms Authentication cookie from the CURRENT request's
        /// HttpContext. Since HttpContext is request-scoped, this ensures that each request gets its
        /// own user's cookie, and cookies are NEVER shared across different user requests.
        /// 
        /// The cookie is read fresh on each call, ensuring:
        /// 1. Always gets the current request's cookie
        /// 2. No possibility of stale or cached cookies
        /// 3. No possibility of cookie from different user
        /// </summary>
        /// <returns>Cookie header value (e.g., ".ASPXAUTH=xyz...") or null if not found</returns>
        /// <exception cref="InvalidOperationException">Thrown when HttpContext is null (shouldn't happen in normal operation)</exception>
        protected virtual string GetAuthCookieFromContext()
        {
            var context = _httpContextAccessor.HttpContext;

            if (context == null)
            {
                Logger.LogError("HttpContext is null when trying to get auth cookie. This should not happen in a normal HTTP request.");
                throw new InvalidOperationException(
                    "HttpContext is not available. This service requires an active HTTP request context.");
            }

            // Get the Forms Authentication cookie from the current request
            if (context.Request.Cookies.TryGetValue(_authCookieName, out var cookieValue))
            {
                // Format as cookie header: "CookieName=CookieValue"
                var cookieHeader = $"{_authCookieName}={cookieValue}";

                Logger.LogDebug(
                    "Retrieved auth cookie from HTTP context for {ServiceType}",
                    typeof(TService).Name);

                return cookieHeader;
            }

            Logger.LogWarning(
                "Authentication cookie '{CookieName}' not found in HTTP context for {ServiceType}. User may not be authenticated.",
                _authCookieName,
                typeof(TService).Name);

            throw new InvalidOperationException(
                $"Authentication cookie '{_authCookieName}' not found. User must be authenticated to use this service.");
        }

        /// <summary>
        /// Creates an authenticated HTTP request using the cookie from the current HTTP context.
        /// 
        /// CRITICAL SECURITY: The authentication cookie is retrieved from the CURRENT request's
        /// HttpContext, ensuring it is never shared across different user requests. Each call
        /// creates a new HttpRequestMessage with the correct user's cookie.
        /// </summary>
        /// <param name="method">HTTP method</param>
        /// <param name="requestUri">Request URI (relative to base address)</param>
        /// <returns>A new HttpRequestMessage with the authentication cookie header</returns>
        protected HttpRequestMessage CreateAuthenticatedRequest(
            HttpMethod method,
            string requestUri)
        {
            // Get cookie from current HTTP context (request-scoped)
            var authCookie = GetAuthCookieFromContext();

            var request = new HttpRequestMessage(method, requestUri);

            // Add the Forms Authentication cookie to THIS REQUEST ONLY
            // This ensures the cookie is not shared across requests from different users
            request.Headers.Add("Cookie", authCookie);

            return request;
        }

        /// <summary>
        /// Sends an authenticated request and deserializes the JSON response.
        /// Authentication cookie is automatically retrieved from the current HTTP context.
        /// Handles common HTTP status codes (401, 403, 404) and provides consistent error handling.
        /// </summary>
        /// <typeparam name="TResponse">The expected response type</typeparam>
        /// <param name="method">HTTP method</param>
        /// <param name="requestUri">Request URI</param>
        /// <param name="operationName">Name of the operation (for logging)</param>
        /// <param name="content">Optional request content</param>
        /// <returns>The deserialized response, or null if the request was unsuccessful</returns>
        protected async Task<TResponse?> SendAuthenticatedRequestAsync<TResponse>(
            HttpMethod method,
            string requestUri,
            string operationName,
            HttpContent? content = null) where TResponse : notnull
        {
            try
            {
                Logger.LogInformation("Executing {OperationName}: {Method} {RequestUri}",
                    operationName, method, requestUri);

                // Create request with cookie from current HTTP context
                var request = CreateAuthenticatedRequest(method, requestUri);

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
                    return default;
                }

                if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    Logger.LogWarning("{OperationName} failed: Forbidden (403) for {RequestUri}",
                        operationName, requestUri);
                    return default;
                }

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    Logger.LogWarning("{OperationName} failed: Not Found (404) for {RequestUri}",
                        operationName, requestUri);
                    return default;
                }

                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<TResponse>(responseContent, JsonOptions);

                if (result == null)
                {
                    Logger.LogError("{OperationName}: Failed to deserialize response from {RequestUri}",
                        operationName, requestUri);
                    return default;
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

        protected async Task SendAuthenticatedRequestAsync(
            HttpMethod method,
            string requestUri,
            string operationName,
            HttpContent? content = null) 
        {
            try
            {
                Logger.LogInformation("Executing {OperationName}: {Method} {RequestUri}",
                    operationName, method, requestUri);

                // Create request with cookie from current HTTP context
                var request = CreateAuthenticatedRequest(method, requestUri);

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
                    return;
                }

                if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    Logger.LogWarning("{OperationName} failed: Forbidden (403) for {RequestUri}",
                        operationName, requestUri);
                    return;
                }

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    Logger.LogWarning("{OperationName} failed: Not Found (404) for {RequestUri}",
                        operationName, requestUri);
                    return;
                }

                response.EnsureSuccessStatusCode();

                Logger.LogInformation("{OperationName} completed successfully", operationName);
                return;
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
        /// Authentication cookie is automatically retrieved from the current HTTP context.
        /// Useful for validation endpoints that don't return data.
        /// </summary>
        /// <param name="method">HTTP method</param>
        /// <param name="requestUri">Request URI</param>
        /// <param name="operationName">Name of the operation (for logging)</param>
        /// <returns>True if successful (2xx status), false otherwise</returns>
        protected async Task<bool> SendAuthenticatedValidationRequestAsync(
            HttpMethod method,
            string requestUri,
            string operationName)
        {
            try
            {
                Logger.LogInformation("Executing {OperationName}: {Method} {RequestUri}",
                    operationName, method, requestUri);

                // Create request with cookie from current HTTP context
                var request = CreateAuthenticatedRequest(method, requestUri);
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
        /// Sends an unauthenticated request (for endpoints like login that don't require cookies,
        /// or for APIs that use service-level authentication like API keys).
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