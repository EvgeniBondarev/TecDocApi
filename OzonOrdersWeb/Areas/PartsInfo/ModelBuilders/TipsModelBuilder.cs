using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using OzonOrdersWeb.Areas.PartsInfo.Models.ABCP;

namespace OzonOrdersWeb.Areas.PartsInfo.ModelBuilders;

public class TipsModelBuilder
{
    private readonly IMemoryCache _memoryCache;
        
    public TipsModelBuilder(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }
    public IEnumerable<TipModel> BuildModel(JsonDocument json, string originalNumber)
    {
        var cacheKey = $"BuildTipsModel_{originalNumber}";
        if (_memoryCache.TryGetValue(cacheKey, out List<TipModel> cachedResult))
        {
            return cachedResult;
        }
        var tips = new List<TipModel>();
        foreach (var item in json.RootElement.EnumerateArray())
        {
            tips.Add(new TipModel
            {
                Brand = item.TryGetProperty("brand", out var brandProp) 
                    ? brandProp.GetString() ?? string.Empty 
                    : string.Empty,
                Number = item.TryGetProperty("number", out var numberProp) 
                    ? numberProp.GetString() ?? string.Empty 
                    : string.Empty,
                Description = item.TryGetProperty("description", out var descProp) 
                    ? descProp.GetString() ?? string.Empty 
                    : string.Empty
            });
        }
        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(30));
        _memoryCache.Set(cacheKey, tips, cacheEntryOptions);
        return tips;
    }
}