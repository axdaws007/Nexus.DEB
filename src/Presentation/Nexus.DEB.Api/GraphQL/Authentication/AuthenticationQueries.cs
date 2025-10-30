using HotChocolate.Authorization;
using Nexus.DEB.Api.GraphQL.Authentication.Models;
using System.Security.Claims;

namespace Nexus.DEB.Api.GraphQL.Authentication
{
    [QueryType]
    public static class AuthenticationQueries
    {
        /// <summary>
        /// Get current user information from the authentication cookie
        /// Requires authentication
        /// </summary>
        [Authorize] // Requires user to be authenticated
        public static CurrentUserInfo GetCurrentUser(ClaimsPrincipal claimsPrincipal)
        {
            var postIdClaim = claimsPrincipal.FindFirst("PostId")?.Value;
            var userIdClaim = claimsPrincipal.FindFirst("UserId")?.Value;
            var nameClaim = claimsPrincipal.FindFirst(ClaimTypes.Name)?.Value;

            return new CurrentUserInfo
            {
                PostId = !string.IsNullOrEmpty(postIdClaim) ? Guid.Parse(postIdClaim) : Guid.Empty,
                UserId = !string.IsNullOrEmpty(userIdClaim) ? Guid.Parse(userIdClaim) : Guid.Empty,
                Username = nameClaim ?? string.Empty,
                IsAuthenticated = claimsPrincipal.Identity?.IsAuthenticated ?? false
            };
        }

        /// <summary>
        /// Check if user is authenticated (no authorization required)
        /// </summary>
        public static bool IsAuthenticated(ClaimsPrincipal claimsPrincipal)
        {
            return claimsPrincipal.Identity?.IsAuthenticated ?? false;
        }
    }
}
