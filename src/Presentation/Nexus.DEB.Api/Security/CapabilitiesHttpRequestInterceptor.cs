using Microsoft.AspNetCore.Http;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Domain;
using Nexus.DEB.Infrastructure.Helpers;
using System.Security.Claims;

namespace Nexus.DEB.Api.Security
{
    public class CapabilitiesHttpRequestInterceptor(RequestDelegate next)
    {
        readonly RequestDelegate _next = next;

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.User?.Identity?.IsAuthenticated == true)
            {
                var applicationSettingsService = context.RequestServices.GetRequiredService<IApplicationSettingsService>();
                var cisService = context.RequestServices.GetRequiredService<ICisService>();
                var cbacService = context.RequestServices.GetRequiredService<ICbacService>();

                var claimsIdentity = new ClaimsIdentity("Capabilities");

                var token = context.User.FindFirst(ClaimTypes.Name)?.Value;

                var result = TokenParser.ParseCookieToken(token);

                if (result.IsSuccess)
                {
                    var (postId, userId) = result.Data;

                    if (userId != Guid.Empty && postId != Guid.Empty)
                    {
                        var userDetails = await cisService.GetUserDetailsAsync(userId, postId);

                        claimsIdentity.AddClaim(new Claim(DebHelper.ClaimTypes.UserId, userId.ToString()));
                        claimsIdentity.AddClaim(new Claim(DebHelper.ClaimTypes.PostId, postId.ToString()));
                        claimsIdentity.AddClaim(new Claim(DebHelper.ClaimTypes.FirstName, userDetails.FirstName));
                        claimsIdentity.AddClaim(new Claim(DebHelper.ClaimTypes.LastName, userDetails.LastName));
                        claimsIdentity.AddClaim(new Claim(DebHelper.ClaimTypes.UserName, userDetails.UserName));
                        claimsIdentity.AddClaim(new Claim(DebHelper.ClaimTypes.PostTitle, userDetails.PostTitle));

                        var moduleId = applicationSettingsService.GetModuleId("DEB");
                        var capabilities = await cbacService.GetCapabilitiesAsync(moduleId);

                        foreach (var capability in capabilities)
                            claimsIdentity.AddClaim(new Claim(DebHelper.ClaimTypes.Capability, capability.CapabilityName));

                        context.User.AddIdentity(claimsIdentity);
                    }
                }
            }

            await _next(context);
        }
    }
}
