using Microsoft.Extensions.Caching.Memory;
using Servcies.CacheServcies.Cache.OrderSummaryCache;

public class OrderSummaryCache : IOrderSummaryCache
{
    private readonly IMemoryCache _cache;

    public OrderSummaryCache(IMemoryCache cache)
    {
        _cache = cache;
    }

    private string GetKey(int cartId) => $"Cart_{cartId}";

    public void Set(int cartId, string htmlTable, TimeSpan? lifetime = null)
    {
        var cacheEntryOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = lifetime ?? TimeSpan.FromHours(1)
        };

        _cache.Set(GetKey(cartId), htmlTable, cacheEntryOptions);
    }

    public string? Get(int cartId)
    {
        return _cache.TryGetValue(GetKey(cartId), out string? value) ? value : null;
    }

    public void Remove(int cartId)
    {
        _cache.Remove(GetKey(cartId));
    }
}