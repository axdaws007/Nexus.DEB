using HotChocolate.Authorization;
using Nexus.DEB.Api.GraphQL.Authentication.Models;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models;
using System.Security.Claims;

namespace Nexus.DEB.Api.GraphQL.Authentication
{
    /// <summary>
    /// GraphQL queries for authentication and user information.
    /// 
    /// SECURITY: Services (CisService, CbacService) automatically retrieve authentication
    /// cookies from HttpContext, ensuring cookies are never shared across different user requests.
    /// </summary>
    [QueryType]
    public static class AuthenticationQueries
    {
        public static bool IsAuthenticated(ClaimsPrincipal claimsPrincipal)
        {
            return claimsPrincipal.Identity?.IsAuthenticated ?? false;
        }

        [Authorize]
        public static async Task<CurrentUserInfo?> GetCurrentUserProfile(ICurrentUserService currentUserService)
        {
            var userDetails = await currentUserService.GetUserDetailsAsync();

            if (userDetails == null)
                return null;

            return new CurrentUserInfo
            {
                UserId = userDetails.UserId,
                UserName = userDetails.UserName,
                FirstName = userDetails.FirstName,
                LastName = userDetails.LastName,
                FullName = userDetails.FullName,
                Email = userDetails.Email,
                PostId = userDetails.PostId,
                PostTitle = userDetails.PostTitle,
                IsAuthenticated = currentUserService.IsAuthenticated
            };
        }

        /// <summary>
        /// Gets capabilities (permissions) for the current user in the DEB module.
        /// 
        /// SECURITY: CbacService automatically retrieves the authentication cookie
        /// from the current HTTP context, so we don't need to pass it here.
        /// </summary>
        public static async Task<List<CbacCapability>> GetCapabilities(
            [Service] ICbacService cbacApi,
            [Service] IConfiguration configuration)
        {
            var moduleIdString = configuration["Modules:DEB"]
                ?? throw new InvalidOperationException("Modules:DEB not configured in appsettings");

            if (!Guid.TryParse(moduleIdString, out var moduleId))
            {
                throw new InvalidOperationException("Modules:DEB must be a valid GUID");
            }

            // CbacService will automatically get the auth cookie from HttpContext!
            // No need to manually extract it anymore.
            return await cbacApi.GetCapabilitiesAsync(moduleId);
        }
    }
}