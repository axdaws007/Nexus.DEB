using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models;

namespace Nexus.DEB.Infrastructure.Services
{
    public class CachedCisService : ICisService
    {
        private readonly ICisService _innerCisService;
        private readonly IMemoryCache _cache;
        private readonly IConfiguration _configuration;
        private readonly ILogger<CachedCisService> _logger;

        // Cache expiration - adjust as needed
        private readonly TimeSpan _cacheExpiration;

        public CachedCisService(ICisService innerCisService, IMemoryCache cache, IConfiguration configuration, ILogger<CachedCisService> logger)
        {
            _innerCisService = innerCisService;
            _cache = cache;
            _configuration = configuration;
            _logger = logger;

            var cacheDuration = _configuration.GetValue<int>("CacheSettings:UserDetailsCacheDurationMinutes", 60);
            _cacheExpiration = TimeSpan.FromMinutes(cacheDuration);
        }

        public async Task<ICollection<PostDetails>?> GetAllPosts()
        {
            return await _innerCisService.GetAllPosts();
        }

        public async Task<ICollection<CisGroup>?> GetAllGroups()
        {
            return await _innerCisService.GetAllGroups();
        }

        public async Task<IReadOnlyDictionary<Guid, string?>> GetNamesByIdsAsync(List<Guid> ids, CancellationToken cancellationToken = default)
        {
            return await _innerCisService.GetNamesByIdsAsync(ids, cancellationToken);
        }

        public async Task<ICollection<PostDetails>?> GetPostsBySearchTextAsync(string searchText, CancellationToken cancellationToken = default)
        {
            return await _innerCisService.GetPostsBySearchTextAsync(searchText, cancellationToken);
        }

        public async Task<UserDetails?> GetUserDetailsAsync(Guid userId, Guid postId)
        {
            if (userId == Guid.Empty || postId == Guid.Empty)
            {
                _logger.LogWarning("Cannot cache user details for where the user and post are not selected.");
                // Don't cache for unauthenticated users
                return await _innerCisService.GetUserDetailsAsync(userId, postId);
            }

            var cacheKey = $"user_details_{userId}_{postId}";

            if (_cache.TryGetValue<UserDetails>(cacheKey, out var cachedUserDetails))
            {
                _logger.LogDebug("Cache hit for capabilities: UserId={UserId}, PostId={PostId}", userId, postId);
                return cachedUserDetails!;
            }

            _logger.LogDebug("Cache miss for capabilities: UserId={UserId}, PostId={PostId}", userId, postId);

            var userDetails = await _innerCisService.GetUserDetailsAsync(userId, postId);

            // Cache the result with sliding expiration
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(_cacheExpiration)
                .SetSize(1); // If using size limit

            _cache.Set(cacheKey, userDetails, cacheEntryOptions);

            return userDetails;
        }

        public async Task<CisUser?> ValidateCredentialsAsync(string username, string password)
        {
            return await _innerCisService.ValidateCredentialsAsync(username, password);
        }

        public async Task<bool> ValidatePostAsync(Guid userId, Guid postId)
        {
            return await _innerCisService.ValidatePostAsync(userId, postId);
        }

        public void InvalidateUserCache(Guid userId, Guid postId)
        {
            var cacheKey = $"user_details_{userId}_{postId}";
            _cache.Remove(cacheKey);
            _logger.LogInformation("Invalidated user details cache for UserId={UserId}, PostId={PostId}",
                userId, postId);

            cacheKey = $"cbac_capabilities_{userId}_{postId}";
            _cache.Remove(cacheKey);
            _logger.LogInformation("Invalidated capabilities cache for UserId={UserId}, PostId={PostId}",
                userId, postId);

        }
    }
}
