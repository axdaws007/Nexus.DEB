using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models;
using System.Security.Claims;

namespace Nexus.DEB.Infrastructure.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICisService _cisService;
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<CurrentUserService> _logger;
        private readonly string _authCookieName;

        public CurrentUserService(
            IHttpContextAccessor httpContextAccessor,
            ICisService userValidationService,
            IMemoryCache memoryCache,
            IConfiguration configuration,
            ILogger<CurrentUserService> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _cisService = userValidationService;
            _memoryCache = memoryCache;
            _logger = logger;
            _authCookieName = configuration["Authentication:CookieName"]
                ?? throw new InvalidOperationException("Authentication:CookieName is not configured");
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

        public bool IsAuthenticated
        {
            get
            {
                return _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
            }
        }

        public async Task<UserDetails?> GetUserDetailsAsync()
        {
            if (!IsAuthenticated)
            {
                _logger.LogWarning("Cannot get user details - user is not authenticated");
                return null;
            }

            var userId = UserId;
            var postId = PostId;

            if (userId == Guid.Empty)
            {
                _logger.LogWarning("Cannot get user details - UserId is empty");
                return null;
            }

            // Create cache key based on user and post
            var cacheKey = $"UserDetails_{userId}_{postId}";

            // Try to get from cache first
            if (_memoryCache.TryGetValue(cacheKey, out UserDetails? cachedDetails))
            {
                _logger.LogDebug("User details retrieved from cache for UserId: {UserId}", userId);
                return cachedDetails;
            }

            // Not in cache - fetch from CIS API
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext == null)
                {
                    _logger.LogError("HttpContext is null - cannot retrieve auth cookie");
                    return null;
                }

                // Get the auth cookie to forward to CIS API
                var authCookie = httpContext.Request.Cookies[_authCookieName];

                if (string.IsNullOrEmpty(authCookie))
                {
                    _logger.LogError("Auth cookie not found - cannot fetch user details");
                    return null;
                }

                var cookieHeader = $"{_authCookieName}={authCookie}";

                // Fetch from CIS API
                var userDetails = await _cisService.GetUserDetailsAsync(userId, postId, cookieHeader);

                if (userDetails != null)
                {
                    // Cache for 5 minutes
                    var cacheOptions = new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
                        SlidingExpiration = TimeSpan.FromMinutes(2)
                    };

                    _memoryCache.Set(cacheKey, userDetails, cacheOptions);
                    _logger.LogInformation("User details cached for {Username}", userDetails.UserName);
                }

                return userDetails;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching user details for UserId: {UserId}", userId);
                return null;
            }
        }
    }
}
