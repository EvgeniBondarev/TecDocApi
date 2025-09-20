using Microsoft.Extensions.Caching.Memory;
using Servcies.DataServcies;
using Order = OzonDomains.Models.Order;

namespace Servcies.CacheServcies.Cache.OzonOrdersCache
{
    public class OrderCache : IRedisAppCache<Order>
    {
        private readonly IMemoryCache _memoryCache;
        private readonly OrdersDataServcies _ordersService;
        private const string CacheKey = "Orders";
        private const int InitialLoadCount = 500;
        private const int IncrementCount = 100;
        private int _maxPage = 1;
        private int _maxItemsToLoad = InitialLoadCount;

        public OrderCache(IMemoryCache memoryCache, OrdersDataServcies ordersDataServcies)
        {
            _memoryCache = memoryCache;
            _ordersService = ordersDataServcies;
        }

        private void SetCache(string key, object value, TimeSpan expiration)
        {
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(expiration);
            _memoryCache.Set(key, value, cacheEntryOptions);
        }

        private T GetCache<T>(string key)
        {
            return _memoryCache.TryGetValue(key, out T value) ? value : default;
        }

        public async Task<List<Order>> Get(int page)
        {
            var cachedOrders = GetCache<List<Order>>(CacheKey);
            if (cachedOrders == null)
            {
                cachedOrders = (await _ordersService.GetOrders(0, InitialLoadCount)).ToList();
                _maxPage = 10;
                SetCache(CacheKey, cachedOrders, TimeSpan.FromMinutes(10));
            }

            int requiredItemCount = page * IncrementCount;
            if (cachedOrders.Count < requiredItemCount)
            {
                int itemsToLoad = requiredItemCount - cachedOrders.Count;
                var additionalOrders = (await _ordersService.GetOrders(cachedOrders.Count, itemsToLoad)).ToList();

                if (additionalOrders.Any())
                {
                    cachedOrders.AddRange(additionalOrders);
                    SetCache(CacheKey, cachedOrders, TimeSpan.FromMinutes(10));
                }

                _maxPage = Math.Max(_maxPage, page);
                _maxItemsToLoad = itemsToLoad;
            }

            return cachedOrders
                .Take(requiredItemCount)
                .OrderBy(o => o.ProcessingDate)
                .ToList();
        }

        public async Task<List<Order>> Get()
        {
            var cachedOrders = GetCache<List<Order>>(CacheKey);
            if (cachedOrders != null)
                return cachedOrders.OrderBy(o => o.ProcessingDate).ToList();

            await Update();
            cachedOrders = GetCache<List<Order>>(CacheKey);

            if (cachedOrders != null)
                return cachedOrders.OrderBy(o => o.ProcessingDate).ToList();

            throw new InvalidOperationException("Данные не найдены в кэше после обновления.");
        }

        public async Task Update()
        {
            var orders = (await _ordersService.GetOrders(0, _maxItemsToLoad)).ToList();
            SetCache(CacheKey, orders, TimeSpan.FromMinutes(10));
        }

        public async Task UpdateCacheIncrementally(int orderId)
        {
            var updatedOrder = await _ordersService.GetOrder(orderId);
            if (updatedOrder == null) return;

            var cachedOrders = GetCache<List<Order>>(CacheKey) ?? new List<Order>();
            var existingOrderIndex = cachedOrders.FindIndex(o => o.Id == orderId);

            if (existingOrderIndex != -1)
            {
                cachedOrders[existingOrderIndex] = updatedOrder;
            }
            else
            {
                cachedOrders.Add(updatedOrder);
            }

            SetCache(CacheKey, cachedOrders, TimeSpan.FromMinutes(10));
        }

        public async Task RemoveOrderFromCache(int orderId)
        {
            var cachedOrders = GetCache<List<Order>>(CacheKey);
            if (cachedOrders == null) return;

            cachedOrders.RemoveAll(o => o.Id == orderId);
            SetCache(CacheKey, cachedOrders, TimeSpan.FromMinutes(10));
        }

        public async Task RemoveOrdersFromCache(int[] orderIds)
        {
            var cachedOrders = GetCache<List<Order>>(CacheKey);
            if (cachedOrders == null) return;

            cachedOrders.RemoveAll(o => orderIds.Contains(o.Id));
            SetCache(CacheKey, cachedOrders, TimeSpan.FromMinutes(10));
        }
    }
}