using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models;
using System.Net.Http.Json;

namespace Nexus.DEB.Infrastructure.Services
{
    /// <summary>
    /// HTTP client wrapper for the legacy .NET Framework CBAC (Capability-Based Access Control) API.
    /// 
    /// SECURITY: Authentication cookies are retrieved from HttpContext (request-scoped),
    /// ensuring cookies are NEVER shared across different user requests.
    /// </summary>
    public class CbacService : LegacyApiServiceBase<CbacService>, ICbacService
    {
        protected override string HttpClientName => "CbacApi";

        public CbacService(
            IHttpClientFactory httpClientFactory,
            ILogger<CbacService> logger,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration)
            : base(httpClientFactory, logger, httpContextAccessor, configuration)
        {
        }

        /// <summary>
        /// Gets the capabilities (permissions) for a specific module.
        /// Authentication cookie is automatically retrieved from the current HTTP context.
        /// </summary>
        public async Task<List<CbacCapability>> GetCapabilitiesAsync(Guid moduleId)
        {
            var requestUri = $"api/Capabilities?moduleId={moduleId}";

            // Use authenticated request - cookie retrieved from HttpContext automatically
            var capabilities = await SendAuthenticatedRequestAsync<List<CbacCapability>>(
                HttpMethod.Get,
                requestUri,
                operationName: $"GetCapabilities for module: {moduleId}");

            // Return empty list if null (consistent with original implementation)
            return capabilities ?? new List<CbacCapability>();
        }

        public async Task<ICollection<Guid>?> GetRolePostIdsAsync(ICollection<Guid> roleIds)
        {
            var requestUri = $"api/Roles/PostIdsForRoles";

            // Create JSON content (JsonOptions from base class)
            var content = JsonContent.Create(roleIds, options: JsonOptions);

            // Use authenticated request - cookie retrieved from HttpContext automatically
            var postIds = await SendAuthenticatedRequestAsync<ICollection<Guid>>(
                HttpMethod.Post,
                requestUri,
                operationName: $"PostIdsForRoles for roles",
                content: content
                );

            // Return empty list if null (consistent with original implementation)
            return postIds;
        }
    }
}