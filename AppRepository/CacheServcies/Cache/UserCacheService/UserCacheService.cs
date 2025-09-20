using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using OzonRepositories.Context.Identity;
using System.Security.Claims;


namespace Servcies.CacheServcies.Cache.UserCacheService
{
    public class UserCacheService : IUserCacheService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly UserManager<CustomIdentityUser> _userManager;

        public UserCacheService(IMemoryCache memoryCache, UserManager<CustomIdentityUser> userManager)
        {
            _memoryCache = memoryCache;
            _userManager = userManager;
        }

        public async Task<CustomIdentityUser> GetCachedUserAsync(ClaimsPrincipal user)
        {
            var userId = _userManager.GetUserId(user);
            if (string.IsNullOrEmpty(userId))
            {
                return null;
            }

            if (!_memoryCache.TryGetValue(userId, out CustomIdentityUser cachedUser))
            {
                cachedUser = await _userManager.GetUserAsync(user);

                if (cachedUser != null)
                {
                    _memoryCache.Set(userId, cachedUser, TimeSpan.FromMinutes(10)); 
                }
            }

            return cachedUser;
        }

        public void InvalidateUserCache(string userId)
        {
            _memoryCache.Remove(userId); 
        }
    }

}
