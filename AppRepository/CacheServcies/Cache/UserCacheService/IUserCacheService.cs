using OzonRepositories.Context.Identity;
using System.Security.Claims;

namespace Servcies.CacheServcies.Cache.UserCacheService
{
    public interface IUserCacheService
    {
        Task<CustomIdentityUser> GetCachedUserAsync(ClaimsPrincipal user);
        void InvalidateUserCache(string userId);
    }

}
