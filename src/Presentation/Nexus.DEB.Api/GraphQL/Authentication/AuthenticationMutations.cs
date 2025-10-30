using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Nexus.DEB.Api.GraphQL.Authentication.Models;
using Nexus.DEB.Application.Common.Interfaces;

namespace Nexus.DEB.Api.GraphQL.Authentication
{
    [MutationType]
    public static class AuthenticationMutations
    {
        /// <summary>
        /// Sign in mutation - authenticates user and creates Forms Authentication cookie
        /// </summary>
        public static async Task<SignInPayload> SignIn(
            SignInInput input,
            [Service] ILoginService loginService)
        {
            var result = await loginService.SignInAsync(
                input.Username,
                input.Password,
                input.RememberMe);

            if (result.IsSuccess && result.Data != null)
            {
                return new SignInPayload
                {
                    UserId = result.Data.UserId,
                    PostId = result.Data.PostId,
                    Username = result.Data.Username,
                    Success = true,
                    Message = "Successfully signed in",
                    ExpiresAt = result.Data.ExpiresAt
                };
            }

            // Handle validation errors - return as GraphQL errors
            if (result.Errors.Any())
            {
                var errors = result.Errors.Select(e =>
                    ErrorBuilder.New()
                        .SetMessage(e.Message)
                        .SetCode(e.Code)
                        .SetExtension("field", e.Field)
                        .SetExtension("meta", e.Meta)
                        .Build());

                throw new GraphQLException(errors);
            }

            // Fallback error
            return new SignInPayload
            {
                Success = false,
                Message = "Sign in failed"
            };
        }

        /// <summary>
        /// Sign out mutation - removes the Forms Authentication cookie
        /// </summary>
        public static async Task<SignOutPayload> SignOut(
            [Service] IHttpContextAccessor httpContextAccessor)
        {
            var httpContext = httpContextAccessor.HttpContext;

            if (httpContext == null)
            {
                return new SignOutPayload
                {
                    Success = false,
                    Message = "Unable to access HTTP context"
                };
            }

            await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return new SignOutPayload
            {
                Success = true,
                Message = "Successfully signed out"
            };
        }
    }
}
