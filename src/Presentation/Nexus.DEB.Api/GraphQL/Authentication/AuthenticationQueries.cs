using HotChocolate.Authorization;
using Nexus.DEB.Api.GraphQL.Authentication.Models;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models;
using System.Security.Claims;

namespace Nexus.DEB.Api.GraphQL
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
        public static async Task<CurrentUserInfo?> GetCurrentUserProfile(
            ICurrentUserService currentUserService,
            ICbacService cbacApi,
            IApplicationSettingsService appSettingsService)
        {
            var userDetails = await currentUserService.GetUserDetailsAsync();

            if (userDetails == null)
                return null;

            List<CbacCapability>? capabilities = null;

            if (userDetails.PostId != Guid.Empty)
            {
                var moduleId = appSettingsService.GetModuleId("DEB");

                capabilities = await cbacApi.GetCapabilitiesAsync(moduleId);
            }

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
                Posts = userDetails.Posts,
                Capabilities = capabilities,
                IsAuthenticated = currentUserService.IsAuthenticated
            };
        }

        /// <summary>
        /// Gets capabilities (permissions) for the current user in the DEB module.
        /// 
        /// SECURITY: CbacService automatically retrieves the authentication cookie
        /// from the current HTTP context, so we don't need to pass it here.
        /// </summary>
        [Authorize]
        public static async Task<List<CbacCapability>> GetCapabilities(
            ICbacService cbacApi,
            IApplicationSettingsService applicationSettingsService)
        {
            var moduleId = applicationSettingsService.GetModuleId("DEB");

            // CbacService will automatically get the auth cookie from HttpContext!
            // No need to manually extract it anymore.
            return await cbacApi.GetCapabilitiesAsync(moduleId);
        }
    }
}