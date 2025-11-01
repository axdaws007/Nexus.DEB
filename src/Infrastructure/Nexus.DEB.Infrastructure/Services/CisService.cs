using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models;
using System.Net;
using System.Net.Http.Json;

namespace Nexus.DEB.Infrastructure.Services
{
    /// <summary>
    /// HTTP client wrapper for the legacy .NET Framework 4.8 CIS Identity Web API.
    /// 
    /// SECURITY: Authentication cookies are retrieved from HttpContext (request-scoped),
    /// ensuring cookies are NEVER shared across different user requests.
    /// </summary>
    public class CisService : LegacyApiServiceBase<CisService>, ICisService
    {
        protected override string HttpClientName => "CisApi";

        public CisService(
            IHttpClientFactory httpClientFactory,
            ILogger<CisService> logger,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration)
            : base(httpClientFactory, logger, httpContextAccessor, configuration)
        {
        }

        /// <summary>
        /// Validates user credentials (login).
        /// This method does NOT require authentication (it's the login endpoint).
        /// </summary>
        public async Task<CisUser?> ValidateCredentialsAsync(string username, string password)
        {
            var requestUri = $"api/Users/Signin?userName={Uri.EscapeDataString(username)}&password={Uri.EscapeDataString(password)}";

            // Use unauthenticated request (this is the login endpoint)
            return await SendUnauthenticatedRequestAsync<CisUser>(
                HttpMethod.Post,
                requestUri,
                operationName: $"ValidateCredentials for user: {username}");
        }

        /// <summary>
        /// Validates that the current user has access to a specific post.
        /// Authentication cookie is automatically retrieved from the current HTTP context.
        /// </summary>
        public async Task<bool> ValidatePostAsync(Guid userId, Guid postId)
        {
            var requestUri = $"api/Users/ValidatePost?postId={postId}";

            // Use authenticated request - cookie retrieved from HttpContext automatically
            return await SendAuthenticatedValidationRequestAsync(
                HttpMethod.Post,
                requestUri,
                operationName: $"ValidatePost {postId} for user {userId}");
        }

        /// <summary>
        /// Gets detailed information about the current user.
        /// Authentication cookie is automatically retrieved from the current HTTP context.
        /// </summary>
        public async Task<UserDetails?> GetUserDetailsAsync(Guid userId, Guid postId)
        {
            var requestUri = $"api/Users/CurrentUser";

            // Use authenticated request - cookie retrieved from HttpContext automatically
            return await SendAuthenticatedRequestAsync<UserDetails>(
                HttpMethod.Get,
                requestUri,
                operationName: $"GetUserDetails for UserId: {userId}, PostId: {postId}");
        }

        public async Task<IReadOnlyDictionary<Guid, string?>> GetNamesByIdsAsync(List<Guid> ids, CancellationToken cancellationToken = default)
        {
            try
            {
                // Create request DTO
                var request = new CisBatchRequest
                {
                    Ids = ids
                };

                // Create JSON content (JsonOptions from base class)
                var content = JsonContent.Create(request, options: JsonOptions);

                // Use base class method - it gets the auth cookie from HttpContext automatically!
                var response = await SendAuthenticatedRequestAsync<CisNamesBatchResponse>(
                    HttpMethod.Post,
                    "api/Users/BatchNames",
                    operationName: $"GetBatchNames for {ids.Count} entities",
                    content: content);

                if (response?.Names == null)
                {
                    Logger.LogWarning("API returned null response for batch status request");
                    return ids.ToDictionary(id => id, id => null as string);
                }

                // Convert API response to dictionary
                var result = new Dictionary<Guid, string?>();
                foreach (var nameItem in response.Names)
                {
                    result[nameItem.Id] = nameItem.Name;
                }

                // Add null entries for any entity IDs that weren't returned
                foreach (var id in ids)
                {
                    if (!result.ContainsKey(id))
                    {
                        result[id] = null;
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error fetching batch workflow statuses");
                throw;
            }
        }
    }
}