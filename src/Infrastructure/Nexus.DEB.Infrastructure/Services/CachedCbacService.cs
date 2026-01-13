using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models;

namespace Nexus.DEB.Infrastructure.Services
{
    public class CachedCbacService : ICbacService
    {
        private readonly ICbacService _innerCbacService;
        private readonly IMemoryCache _cache;
        private readonly ICurrentUserService _currentUserService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<CachedCbacService> _logger;

        // Cache expiration - adjust as needed
        private readonly TimeSpan _cacheExpiration;

        public CachedCbacService(
            ICbacService innerCbacService,
            IMemoryCache cache,
            ICurrentUserService currentUserService,
            IConfiguration configuration,
            ILogger<CachedCbacService> logger)
        {
            _innerCbacService = innerCbacService;
            _cache = cache;
            _currentUserService = currentUserService;
            _logger = logger;
            _configuration = configuration;

            var cacheDuration = _configuration.GetValue<int>("CacheSettings:CapabilitiesCacheDurationMinutes", 15);
            _cacheExpiration = TimeSpan.FromMinutes(cacheDuration);
        }

        public async Task<List<CbacCapability>> GetCapabilitiesAsync(Guid moduleId)
        {
            // Get current user ID for cache key
            var userId = _currentUserService.UserId;
            var postId = _currentUserService.PostId;

            if (userId == Guid.Empty)
            {
                _logger.LogWarning("Cannot cache capabilities for unauthenticated user");
                // Don't cache for unauthenticated users
                return await _innerCbacService.GetCapabilitiesAsync(moduleId);
            }

            // Create cache key: userId + moduleId ensures isolation per user and postId
            var cacheKey = $"cbac_capabilities_{userId}_{postId}";

            // Try to get from cache
            if (_cache.TryGetValue<List<CbacCapability>>(cacheKey, out var cachedCapabilities))
            {
                _logger.LogInformation("Invalidated capabilities cache for UserId={UserId}, PostId={PostId}",
                    userId, postId);
                return cachedCapabilities!;
            }

            _logger.LogDebug("Cache miss for capabilities: UserId={UserId}, PostId={PostId}",
                userId, postId);

            // Fetch from API
            var capabilities = await _innerCbacService.GetCapabilitiesAsync(moduleId);

            // Cache the result with sliding expiration
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(_cacheExpiration)
                .SetSize(1); // If using size limit

            _cache.Set(cacheKey, capabilities, cacheEntryOptions);

            return capabilities;
        }

        public async Task<ICollection<Guid>?> GetRolePostIdsAsync(ICollection<Guid> roleIds)
        {
            // This could also be cached if needed, but capabilities are the priority
            return await _innerCbacService.GetRolePostIdsAsync(roleIds);
        }

        public async Task<ICollection<CbacRole>?> GetRolesForPostAsync(Guid postId)
        {
            return await _innerCbacService.GetRolesForPostAsync(postId);
        }
    }
}
