using Microsoft.Extensions.Logging;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models;

namespace Nexus.DEB.Infrastructure.Services
{
    /// <summary>
    /// HTTP client wrapper for the legacy .NET Framework CBAC (Capability-Based Access Control) API
    /// </summary>
    public class CbacService : LegacyApiServiceBase<CbacService>, ICbacService
    {
        protected override string HttpClientName => "CbacApi";

        public CbacService(
            IHttpClientFactory httpClientFactory,
            ILogger<CbacService> logger)
            : base(httpClientFactory, logger)
        {
        }

        public async Task<List<CbacCapability>> GetCapabilitiesAsync(Guid moduleId, string authCookie)
        {
            var requestUri = $"api/Capabilities?moduleId={moduleId}";

            // Use the base class method for authenticated requests
            var capabilities = await SendAuthenticatedRequestAsync<List<CbacCapability>>(
                HttpMethod.Get,
                requestUri,
                authCookie,
                operationName: $"GetCapabilities for module: {moduleId}");

            // Return empty list if null (consistent with original implementation)
            return capabilities ?? new List<CbacCapability>();
        }
    }
}