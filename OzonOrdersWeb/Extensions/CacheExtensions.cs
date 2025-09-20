using OzonDomains.Models;
using PartsInfo.Cache;
using Servcies.CacheServcies.Cache.CartCache;
using Servcies.CacheServcies.Cache.OrderSummaryCache;
using Servcies.CacheServcies.Cache.OzonOrdersCache;
using Servcies.CacheServcies.Cache.UniqueValuesCache;
using Servcies.CacheServcies.Cache.UserCacheService;
using Services.CacheServcies.Cache;
using Services.CacheServcies.Cache.EtProducerCache;
using Services.CacheServcies.Cache.OzonOrdersCache;

namespace OzonOrdersWeb.Extensions;

public static class CacheExtensions
{
    public static void AddCacheServices(this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddDistributedMemoryCache();
        services.AddSession();

        services.AddSingleton<AppCache>();
        services.AddScoped<OrderCache>();
        services.AddScoped<EtProducerCache>();
        services.AddScoped<ProductCache>();
        services.AddScoped<TransactionCache>();

        services.AddScoped<CacheUpdater<Product>>();
        services.AddScoped<CacheUpdater<Order>>();
        services.AddScoped<CacheUpdater<Transaction>>();

        services.AddScoped<IUniqueValuesCache, UniqueValuesCache>();
        services.AddScoped<IUserCacheService, UserCacheService>();
        
        services.AddScoped<CartCache>();
        
        services.AddSingleton<IOrderSummaryCache, OrderSummaryCache>();
        services.AddScoped<UrlResponseCacheService>();
    }
}