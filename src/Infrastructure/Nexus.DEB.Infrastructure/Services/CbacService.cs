using Microsoft.Extensions.Logging;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace Nexus.DEB.Infrastructure.Services
{
    public class CbacService : ICbacService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CbacService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public CbacService(
            IHttpClientFactory httpClientFactory,
            ILogger<CbacService> logger)
        {
            _httpClient = httpClientFactory.CreateClient("CbacApi");
            _logger = logger;

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<List<CbacCapability>> GetCapabilitiesAsync(Guid moduleId, string authCookie)
        {
            try
            {
                _logger.LogInformation("Fetching capabilities for module: {ModuleId}", moduleId);

                if (string.IsNullOrEmpty(authCookie))
                {
                    _logger.LogError("Auth cookie is missing for GetCapabilities call");
                    throw new InvalidOperationException("Authentication cookie is required");
                }

                var requestUri = $"api/Capabilities?moduleId={moduleId}";

                // Create a new request message for this specific call
                var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

                // Forward the Forms Authentication cookie to the CBAC API
                request.Headers.Add("Cookie", authCookie);

                var response = await _httpClient.SendAsync(request);

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("Unauthorized when fetching capabilities for module: {ModuleId}", moduleId);
                    throw new UnauthorizedAccessException("User is not authorized to access capabilities");
                }

                response.EnsureSuccessStatusCode();

                var capabilities = await response.Content
                    .ReadFromJsonAsync<List<CbacCapability>>(_jsonOptions);

                _logger.LogInformation("Successfully fetched {Count} capabilities for module: {ModuleId}",
                    capabilities?.Count ?? 0, moduleId);

                return capabilities ?? new List<CbacCapability>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex,
                    "HTTP error while fetching capabilities for module: {ModuleId}",
                    moduleId);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to fetch capabilities for module: {ModuleId}",
                    moduleId);
                throw;
            }
        }
    }
}
