using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using OzonOrdersWeb.Areas.PartsInfo.Models.PrPart;

namespace OzonOrdersWeb.Areas.PartsInfo.ModelBuilders;

public class PartsFinderArticleModelBuilder
{
    private readonly IMemoryCache _memoryCache;

    public PartsFinderArticleModelBuilder(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    public async Task<PrArticleFullModel> Build(JsonDocument document, 
                                                string code, 
                                                int supplierId)
    {
        var cacheKey = $"PrArticleFullModel_{code}_{supplierId}";
        
        if (_memoryCache.TryGetValue(cacheKey, out PrArticleFullModel cachedResult))
        {
            return cachedResult;
        }
        
        if (document == null)
            return null;
        var root = document.RootElement;
        if (root.ValueKind != JsonValueKind.Array || root.GetArrayLength() == 0)
            return null;
        

        var element = root[0];
        var result = await Task.FromResult(BuildFromElement(element));
        
        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(30));
        _memoryCache.Set(cacheKey, result, cacheEntryOptions);

        return result;
    }

    public async Task<List<PrArticleFullModel>> BuildFromArrayAsync(JsonDocument document)
    {
        var root = document.RootElement;

        if (root.ValueKind != JsonValueKind.Array)
            return new List<PrArticleFullModel>();

        var list = root.EnumerateArray()
            .Select(BuildFromElement)
            .ToList();

        return await Task.FromResult(list);
    }

    private PrArticleFullModel BuildFromElement(JsonElement element)
    {
        return new PrArticleFullModel
        {
            Article = element.GetProperty("article").GetString(),
            Brand = element.GetProperty("brand").GetString(),
            VendorCode = element.GetProperty("Vendor_Code").GetString(),

            ImgUrls = element.TryGetProperty("images", out var images) && images.ValueKind == JsonValueKind.Array
                ? images.EnumerateArray().Select(i => i.GetString()).Where(i => !string.IsNullOrWhiteSpace(i)).ToList()
                : new List<string>(),

            DetailAttributes = element.TryGetProperty("attributes", out var attributes) && attributes.ValueKind == JsonValueKind.Array
                ? attributes.EnumerateArray().Select(BuildAttribute).ToList()
                : new List<VendorAttributeModel>(),

            VendorCategoryName = element.TryGetProperty("Vendor_Category_Name", out var vcat) ? vcat.GetString() : null,
            OemCode = element.TryGetProperty("OEM_Code", out var oemCode) ? oemCode.GetString() : null,
            OemMark = element.TryGetProperty("OEM_Mark", out var oemMark) ? oemMark.GetString() : null,
            Models = element.TryGetProperty("models", out var models) && models.ValueKind == JsonValueKind.Array
                ? models.EnumerateArray().Select(m => m.GetString()).Where(m => !string.IsNullOrWhiteSpace(m)).ToList()
                : new List<string>()
        };
    }

    private VendorAttributeModel BuildAttribute(JsonElement el) => new()
    {
        Name = el.GetProperty("name").GetString(),
        Value = el.GetProperty("value").GetString()
    };
}
