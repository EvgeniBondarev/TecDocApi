using Services.CacheServcies.Cache;

namespace Servcies.CacheServcies.Cache
{
    public interface ICacheUpdater<T>
    {
        Task Update(IAppCache<T> cacheEntity);
    }
}
