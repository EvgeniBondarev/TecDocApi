using Microsoft.Extensions.Caching.Memory;

namespace Services.CacheServcies.Cache
{
    public class AppCache
    {
        protected readonly IMemoryCache _cache;
        
        public AppCache(IMemoryCache memoryCache)
        {
            _cache = memoryCache;
        }
        
        public string GenerateCacheKey(List<Dictionary<string, string>> table)
        {
            if (table.Count == 0)
            {
                return "empty_table";
            }

            var firstValue = table.FirstOrDefault()?.Values.FirstOrDefault() ?? string.Empty;
            var middleValue = table.ElementAtOrDefault(table.Count / 2)?.Values.FirstOrDefault() ?? string.Empty;
            var lastValue = table.LastOrDefault()?.Values.FirstOrDefault() ?? string.Empty;
            return $"{firstValue}_{middleValue}_{lastValue}";
        }
        
        public IMemoryCache GetCache()
        {
            return _cache;
        }
    }

}
