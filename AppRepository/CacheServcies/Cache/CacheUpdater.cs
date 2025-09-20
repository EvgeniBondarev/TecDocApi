using Servcies.CacheServcies;
using Servcies.CacheServcies.Cache;
using Servcies.CacheServcies.Cache.OzonOrdersCache;
using Services.CacheServcies.Cache.OzonOrdersCache;

namespace Services.CacheServcies.Cache
{
    public class CacheUpdater<T> : ICacheUpdater<T> 
    {
        private readonly OrderCache _orderCache;
        private readonly TransactionCache _transactionCache;


        public CacheUpdater(OrderCache orderCache,
                            TransactionCache transactionCache)
        {
            _orderCache = orderCache;
            _transactionCache = transactionCache;
        }

        public async Task Update(IAppCache<T> cacheEntity)
        {
            await cacheEntity.Update();

            if (typeof(OrderCache).IsAssignableFrom(cacheEntity.GetType()))
            {
                await _transactionCache.Update();
            }
            if (typeof(ProductCache).IsAssignableFrom(cacheEntity.GetType()))
            {
                await _orderCache.Update();
            }

        }
        
        public async Task Update(IRedisAppCache<T> cacheEntity)
        {
            await cacheEntity.Update();

            if (typeof(OrderCache).IsAssignableFrom(cacheEntity.GetType()))
            {
                await _transactionCache.Update();
            }
            if (typeof(ProductCache).IsAssignableFrom(cacheEntity.GetType()))
            {
                await _orderCache.Update();
            }

        }
    }
}
