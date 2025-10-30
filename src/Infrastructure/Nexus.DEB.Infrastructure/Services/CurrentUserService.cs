using Microsoft.AspNetCore.Http;
using Nexus.DEB.Application.Common.Interfaces;
using System.Security.Claims;

namespace Nexus.DEB.Infrastructure.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid UserId
        {
            get
            {
                var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("UserId");
                return userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId)
                    ? userId
                    : Guid.Empty;
            }
        }

        public Guid PostId
        {
            get
            {
                var postIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("PostId");
                return postIdClaim != null && Guid.TryParse(postIdClaim.Value, out var postId)
                    ? postId
                    : Guid.Empty;
            }
        }

        public string? UserName
        {
            get
            {
                return _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value;
            }
        }

        public bool IsAuthenticated
        {
            get
            {
                return _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
            }
        }
    }
}
