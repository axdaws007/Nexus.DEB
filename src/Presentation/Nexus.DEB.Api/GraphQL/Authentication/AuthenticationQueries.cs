using HotChocolate.Authorization;
using Nexus.DEB.Api.GraphQL.Authentication.Models;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models;
using Nexus.DEB.Domain;
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
            ClaimsPrincipal claimsPrincipal,
            IApplicationSettingsService appSettingsService)
        {
            var userDetails = await currentUserService.GetUserDetailsAsync();

            if (userDetails == null)
                return null;

            List<string> capabilities = new List<string>();

            if (userDetails.PostId != Guid.Empty)
            {
                capabilities = claimsPrincipal.Claims.Where(x => x.Type == DebHelper.ClaimTypes.Capability).OrderBy(x => x.Value).Select(x => x.Value).ToList();
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
                Posts = userDetails.Posts?.OrderBy(x => x.Title).ToList(),
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
        public static async Task<List<string>> GetCapabilities(
            ICbacService cbacApi,
            IApplicationSettingsService applicationSettingsService)
        {
            var moduleId = applicationSettingsService.GetModuleId("DEB");

            // CbacService will automatically get the auth cookie from HttpContext!
            // No need to manually extract it anymore.
            var capabilities = await cbacApi.GetCapabilitiesAsync(moduleId);

            return capabilities.Select(x => x.CapabilityName).ToList();
        }
    }
}