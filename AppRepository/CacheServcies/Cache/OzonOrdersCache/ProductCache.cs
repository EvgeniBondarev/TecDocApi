using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using OzonDomains.Models;
using Servcies.CacheServcies;
using Servcies.DataServcies;

namespace Services.CacheServcies.Cache.OzonOrdersCache
{
    public class ProductCache : IRedisAppCache<Product>
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ProductsDataServcies _productServcies;
        private const string CacheKey = "Products";
        private const int CacheExpirationMinutes = 10;

        public ProductCache(IMemoryCache memoryCache, ProductsDataServcies productsDataServcies)
        {
            _memoryCache = memoryCache;
            _productServcies = productsDataServcies;
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

        public async Task<List<Product>> Get()
        {
            var cachedProducts = GetCache<List<Product>>(CacheKey);
            if (cachedProducts != null)
            {
                return cachedProducts;
            }
            var products = await Set();
            return products;
        }

        public async Task<List<Product>> Set()
        {
            var products = await _productServcies.GetProducts();
            SetCache(CacheKey, products, TimeSpan.FromMinutes(CacheExpirationMinutes));
            return products;
        }

        public async Task Update()
        {
            var products = await _productServcies.GetProducts();
            SetCache(CacheKey, products, TimeSpan.FromMinutes(CacheExpirationMinutes));
        }
    }
}