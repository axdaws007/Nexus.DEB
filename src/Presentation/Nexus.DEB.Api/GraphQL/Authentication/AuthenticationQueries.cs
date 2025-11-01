using HotChocolate.Authorization;
using Nexus.DEB.Api.GraphQL.Authentication.Models;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models;
using System.Security.Claims;

namespace Nexus.DEB.Api.GraphQL.Authentication
{
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

        public static async Task<List<CbacCapability>> GetCapabilities(
            [Service] ICbacService cbacApi,
            [Service] IHttpContextAccessor httpContextAccessor,
            [Service] IConfiguration configuration)
        {
            var moduleIdString = configuration["Modules:DEB"]
                ?? throw new InvalidOperationException("Modules:DEB not configured in appsettings");

            if (!Guid.TryParse(moduleIdString, out var moduleId))
            {
                throw new InvalidOperationException("Modules:DEB must be a valid GUID");
            }

            var httpContext = httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                throw new InvalidOperationException("HttpContext is not available");
            }

            var authCookieName = configuration["Authentication:CookieName"] 
                ?? throw new InvalidOperationException("Authentication:CookieName not configured in appsettings");

            // Retrieve the auth cookie from the current request
            var authCookie = httpContext.Request.Cookies[authCookieName];

            if (string.IsNullOrEmpty(authCookie))
            {
                throw new UnauthorizedAccessException("Authentication cookie not found");
            }

            var cookieHeader = $"{authCookieName}={authCookie}";

            return await cbacApi.GetCapabilitiesAsync(moduleId, cookieHeader);
        }
    }
}
