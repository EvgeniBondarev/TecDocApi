using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using OzonOrdersWeb.Areas.PartsInfo.Models.CrossCode;

namespace OzonOrdersWeb.Areas.PartsInfo.ModelBuilders;

public class CrossArticleModelBuilder
{
    private readonly IMemoryCache _memoryCache;

    public CrossArticleModelBuilder(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    public async Task<List<CrossArticleModel>> Build(JsonDocument document, string articleCode)
    {
        var cacheKey = $"CrossArticleModel_{articleCode}";

        if (_memoryCache.TryGetValue(cacheKey, out List<CrossArticleModel> cachedList))
        {
            return cachedList;
        }

        var result = new List<CrossArticleModel>();

        if (document?.RootElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var element in document.RootElement.EnumerateArray())
            {
                result.Add(BuildFromElement(element));
            }
        }

        var cacheOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(30));

        _memoryCache.Set(cacheKey, result, cacheOptions);

        return await Task.FromResult(result);
    }

    private CrossArticleModel BuildFromElement(JsonElement el)
    {
        return new CrossArticleModel
        {
            SupplierId = el.GetProperty("supplierid").GetInt32(),
            DataSupplierArticleNumber = el.GetProperty("datasupplierarticlenumber").GetString(),
            IsAdditive = el.GetProperty("IsAdditive").GetBoolean(),
            OENbr = el.GetProperty("OENbr").GetString(),
            ManufacturerId = el.GetProperty("manufacturerId").GetInt32(),
            Supplier = el.TryGetProperty("supplier", out var s) && s.ValueKind == JsonValueKind.Object
                ? BuildSupplier(s)
                : null,
            Manufacturer = el.TryGetProperty("manufacturer", out var m) && m.ValueKind == JsonValueKind.Object
                ? BuildManufacturer(m)
                : null
        };
    }

    private CrossSupplierModel BuildSupplier(JsonElement el) => new()
    {
        Id = el.GetProperty("id").GetInt32(),
        DataVersion = el.GetProperty("dataversion").GetInt32(),
        Description = el.GetProperty("description").GetString(),
        MatchCode = el.GetProperty("matchcode").GetString(),
        NbrOfArticles = el.GetProperty("nbrofarticles").GetInt64(),
        HasNewVersionArticles = el.GetProperty("hasnewversionarticles").GetBoolean(),
        Img = el.TryGetProperty("img", out var img) && img.ValueKind == JsonValueKind.String
            ? img.GetString()
            : null
    };

    private CrossManufacturerModel BuildManufacturer(JsonElement el) => new()
    {
        Id = el.GetProperty("id").GetInt32(),
        CanBeDisplayed = el.GetProperty("canbedisplayed").GetBoolean(),
        Description = el.GetProperty("description").GetString(),
        FullDescription = el.GetProperty("fulldescription").GetString(),
        HasLink = el.GetProperty("haslink").GetBoolean(),
        IsAxle = el.GetProperty("isaxle").GetBoolean(),
        IsCommercialVehicle = el.GetProperty("iscommercialvehicle").GetBoolean(),
        IsEngine = el.GetProperty("isengine").GetBoolean(),
        IsMotorbike = el.GetProperty("ismotorbike").GetBoolean(),
        IsPassengerCar = el.GetProperty("ispassengercar").GetBoolean(),
        IsTransporter = el.GetProperty("istransporter").GetBoolean(),
        IsVgl = el.GetProperty("isvgl").GetBoolean(),
        MatchCode = el.GetProperty("matchcode").GetString()
    };
}
