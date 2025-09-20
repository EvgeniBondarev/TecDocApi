using Microsoft.Extensions.Caching.Memory;
using OzonDomains.Models.OrderCarts;
using OzonRepositories.Data;

namespace Servcies.CacheServcies.Cache.CartCache
{
    public class CartCache : IRedisAppCache<OrderCart>
    {
        private readonly IMemoryCache _memoryCache;
        private readonly OrderCartRepository _cartRepository;
        private const string CacheKey = "OrderCarts";
        private const string TotalCountCacheKey = "OrderCartTotalCount";
        private const int InitialLoadCount = 10;
        private const int IncrementCount = 5;
        private int _maxItemsToLoad = InitialLoadCount;

        public CartCache(IMemoryCache memoryCache, OrderCartRepository cartRepository)
        {
            _memoryCache = memoryCache;
            _cartRepository = cartRepository;
        }

        public int GetIncrementCount()
        {
            return IncrementCount;
        }

        private void SetCache(string key, object value, TimeSpan expiration)
        {
            var options = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(expiration);
            _memoryCache.Set(key, value, options);
        }

        private T GetCache<T>(string key)
        {
            return _memoryCache.TryGetValue(key, out T value) ? value : default;
        }

        public async Task<List<OrderCart>> Get(int page)
        {
            const int pageSize = IncrementCount;
            string pageKey = $"{CacheKey}_Page_{page}";

            var cachedPage = GetCache<List<OrderCart>>(pageKey);
            if (cachedPage != null)
                return cachedPage;

            var freshData = await _cartRepository.GetWithPagination((page - 1) * pageSize, pageSize);
            SetCache(pageKey, freshData, TimeSpan.FromMinutes(10));

            return freshData;
        }
        
        

        public async Task<List<OrderCart>> Get()
        {
            var cachedCarts = GetCache<List<OrderCart>>(CacheKey);
            if (cachedCarts != null)
                return cachedCarts.OrderByDescending(c => c.CreatedAt).ToList();

            await Update();
            return GetCache<List<OrderCart>>(CacheKey)
                ?.OrderByDescending(c => c.CreatedAt).ToList()
                ?? new List<OrderCart>();
        }

        public async Task Update()
        {
            const int pageSize = IncrementCount;
            string pageKey = $"{CacheKey}_Page_1";
            var freshData = await _cartRepository.GetWithPagination(0, pageSize);
            SetCache(pageKey, freshData, TimeSpan.FromMinutes(10));
        }

        public async Task UpdateCacheIncrementally(OrderCart updatedCart)
        {
            var cachedCarts = GetCache<List<OrderCart>>(CacheKey) ?? new List<OrderCart>();
            var index = cachedCarts.FindIndex(c => c.Id == updatedCart.Id);

            if (index != -1)
                cachedCarts[index] = updatedCart;
            else
                cachedCarts.Add(updatedCart);

            SetCache(CacheKey, cachedCarts, TimeSpan.FromMinutes(10));
        }

        public async Task RemoveCartFromCache(int cartId)
        {
            var cachedCarts = GetCache<List<OrderCart>>(CacheKey);
            if (cachedCarts == null) return;

            cachedCarts.RemoveAll(c => c.Id == cartId);
            SetCache(CacheKey, cachedCarts, TimeSpan.FromMinutes(10));
        }
        
        public async Task<int> GetTotalCount()
        {
            if (_memoryCache.TryGetValue(TotalCountCacheKey, out int cachedCount))
            {
                return cachedCount;
            }
            int count = await _cartRepository.GetTotalCount();
            _memoryCache.Set(TotalCountCacheKey, count, TimeSpan.FromMinutes(10));
            return count;
        }
    }
}
