using Microsoft.Extensions.Caching.Memory;
using Servcies.DataServcies;

namespace Servcies.CacheServcies.Cache.UniqueValuesCache
{
    public class UniqueValuesCache : IUniqueValuesCache
    {
        private readonly IMemoryCache _cache;
        private readonly OrdersDataServcies _orderService;

        private const string CacheKeyArticles = "UniqueArticles";
        private const string CacheKeyDeliveryCities = "UniqueDeliveryCities";
        private const string CacheKeyShipmentNumbers = "UniqueShipmentNumbers";
        private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(10);

        public UniqueValuesCache(IMemoryCache memoryCache, OrdersDataServcies orderService)
        {
            _cache = memoryCache;
            _orderService = orderService;
        }

        public async Task<Dictionary<string, int>> GetUniqueArticles()
        {
            return await GetOrSetCache(CacheKeyArticles, _orderService.GetUniqueArticles);
        }

        public async Task<Dictionary<string, int>> GetUniqueDeliveryCitys()
        {
            return await GetOrSetCache(CacheKeyDeliveryCities, _orderService.GetUniqueDeliveryCities);
        }

        public async Task<Dictionary<string, int>> GetUniqueShipmentNumbers()
        {
            return await GetOrSetCache(CacheKeyShipmentNumbers, _orderService.GetUniqueShipmentNumbers);
        }

        public async Task UpdateCache()
        {
            await GetOrSetCache(CacheKeyArticles, _orderService.GetUniqueArticles, forceUpdate: true);
            await GetOrSetCache(CacheKeyDeliveryCities, _orderService.GetUniqueDeliveryCities, forceUpdate: true);
            await GetOrSetCache(CacheKeyShipmentNumbers, _orderService.GetUniqueShipmentNumbers, forceUpdate: true);
        }

        private async Task<Dictionary<string, int>> GetOrSetCache(
            string cacheKey,
            Func<Task<Dictionary<string, int>>> dataRetrievalFunction,
            bool forceUpdate = false)
        {
            if (!forceUpdate && _cache.TryGetValue(cacheKey, out Dictionary<string, int> cachedData))
            {
                return cachedData;
            }

            var data = await dataRetrievalFunction();
            _cache.Set(cacheKey, data, _cacheDuration);
            return data;
        }
    }

}
