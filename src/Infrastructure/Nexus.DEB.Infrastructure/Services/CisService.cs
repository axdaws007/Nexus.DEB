using Microsoft.Extensions.Logging;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models;

namespace Nexus.DEB.Infrastructure.Services
{
    /// <summary>
    /// HTTP client wrapper for the legacy .NET Framework 4.8 CIS Identity Web API
    /// </summary>
    public class CisService : LegacyApiServiceBase<CisService>, ICisService
    {
        protected override string HttpClientName => "CisApi";

        public CisService(
            IHttpClientFactory httpClientFactory,
            ILogger<CisService> logger)
            : base(httpClientFactory, logger)
        {
        }

        public async Task<CisUser?> ValidateCredentialsAsync(string username, string password)
        {
            // Build the query string - matching your API format
            var requestUri = $"api/Users/Signin?userName={Uri.EscapeDataString(username)}&password={Uri.EscapeDataString(password)}";

            // Use the base class method for unauthenticated requests
            return await SendUnauthenticatedRequestAsync<CisUser>(
                HttpMethod.Post,
                requestUri,
                operationName: $"ValidateCredentials for user: {username}");
        }

        public async Task<bool> ValidatePostAsync(Guid userId, Guid postId, string authCookie)
        {
            var requestUri = $"api/Users/ValidatePost?postId={postId}";

            // Use the base class method for validation requests
            return await SendAuthenticatedValidationRequestAsync(
                HttpMethod.Post,
                requestUri,
                authCookie,
                operationName: $"ValidatePost {postId} for user {userId}");
        }

        public async Task<UserDetails?> GetUserDetailsAsync(Guid userId, Guid postId, string authCookie)
        {
            var requestUri = $"api/Users/CurrentUser";

            // Use the base class method for authenticated requests
            return await SendAuthenticatedRequestAsync<UserDetails>(
                HttpMethod.Get,
                requestUri,
                authCookie,
                operationName: $"GetUserDetails for UserId: {userId}, PostId: {postId}");
        }
    }
}