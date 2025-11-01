using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models;

namespace Nexus.DEB.Infrastructure.Services
{
    /// <summary>
    /// Service for accessing information about the currently authenticated user.
    /// 
    /// SECURITY: This service retrieves user information from the CIS API.
    /// The CisService automatically gets the authentication cookie from HttpContext,
    /// ensuring cookies are never shared across different user requests.
    /// </summary>
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICisService _userValidationService;
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<CurrentUserService> _logger;

        public CurrentUserService(
            IHttpContextAccessor httpContextAccessor,
            ICisService userValidationService,
            IMemoryCache memoryCache,
            ILogger<CurrentUserService> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _userValidationService = userValidationService;
            _memoryCache = memoryCache;
            _logger = logger;
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

        /// <summary>
        /// Gets detailed information about the current user from the CIS API.
        /// Results are cached for 5 minutes to reduce API calls.
        /// 
        /// SECURITY: CisService automatically retrieves the authentication cookie
        /// from the current HTTP context, ensuring the correct user's cookie is used.
        /// </summary>
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
                // CisService will automatically get the auth cookie from HttpContext!
                // No need to pass it as a parameter anymore.
                var userDetails = await _userValidationService.GetUserDetailsAsync(userId, postId);

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