using Microsoft.Extensions.Logging;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models;
using System.Net;
using System.Text.Json;

namespace Nexus.DEB.Infrastructure.Services
{
    /// <summary>
    /// HTTP client wrapper for the legacy .NET Framework 4.8 CIS Identity Web API
    /// </summary>
    public class CisIdentityApiClient : IUserValidationService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CisIdentityApiClient> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public CisIdentityApiClient(
            IHttpClientFactory httpClientFactory,
            ILogger<CisIdentityApiClient> logger)
        {
            _httpClient = httpClientFactory.CreateClient("CisIdentityApi");
            _logger = logger;

            // Configure JSON options to match the API response format
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<CisUser?> ValidateCredentialsAsync(string username, string password)
        {
            try
            {
                _logger.LogInformation("Validating credentials for user: {Username}", username);

                // Build the query string - matching your API format
                var requestUri = $"api/Users/Signin?userName={Uri.EscapeDataString(username)}&password={Uri.EscapeDataString(password)}";

                // Make the HTTP POST request
                var response = await _httpClient.PostAsync(requestUri, null);

                // Handle 401 Unauthorized - invalid credentials
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("Invalid credentials for user: {Username}", username);
                    return null;
                }

                // Ensure success status code (200 OK)
                response.EnsureSuccessStatusCode();

                // Deserialize the response directly to CisUser
                var responseContent = await response.Content.ReadAsStringAsync();
                var cisUser = JsonSerializer.Deserialize<CisUser>(responseContent, _jsonOptions);

                if (cisUser == null)
                {
                    _logger.LogError("Failed to deserialize CIS API response for user: {Username}", username);
                    return null;
                }

                _logger.LogInformation("Successfully validated user: {Username} with {PostCount} posts",
                    username, cisUser.Posts.Count);

                return cisUser;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed while validating credentials for user: {Username}", username);
                throw new InvalidOperationException(
                    $"Failed to communicate with CIS Identity API: {ex.Message}", ex);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse CIS API response for user: {Username}", username);
                throw new InvalidOperationException(
                    $"Failed to parse CIS Identity API response: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while validating credentials for user: {Username}", username);
                throw;
            }
        }

        public async Task<bool> ValidatePostAsync(Guid userId, Guid postId)
        {
            try
            {
                _logger.LogInformation("Validating credentials for user ID: {userId}", userId);

                // Build the query string - matching your API format
                var requestUri = $"api/Users/ValidatePost?userId={userId.ToString()}&postId={postId.ToString()}";

                // Make the HTTP POST request
                var response = await _httpClient.PostAsync(requestUri, null);

                // Handle 401 Unauthorized - invalid credentials
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("Invalid post for user ID: {userId}", userId);
                    return false;
                }

                // Ensure success status code (200 OK)
                response.EnsureSuccessStatusCode();

                return true;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed while validating post for user ID: {userId}", userId);
                throw new InvalidOperationException(
                    $"Failed to communicate with CIS Identity API: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while validating credentials for user ID: {userId}", userId);
                throw;
            }
        }
    }
}