namespace Servcies.CacheServcies.Cache.OrderSummaryCache;

public interface IOrderSummaryCache
{
    void Set(int cartId, string htmlTable, TimeSpan? lifetime = null);
    string? Get(int cartId);
    void Remove(int cartId);
}