using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using OzonOrdersWeb.Areas.PartsInfo.Models.ABCP;

namespace OzonOrdersWeb.Areas.PartsInfo.ModelBuilders;

public class ArticleSearchModelBuilder
{
    private readonly IMemoryCache _memoryCache;
    
    public ArticleSearchModelBuilder(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }
    
    public IEnumerable<ArticleSearchModel> BuildModel(JsonDocument json, string brand, string number)
    {
        var cacheKey = $"ArticleSearch_{brand}_{number}";
        
        if (_memoryCache.TryGetValue(cacheKey, out List<ArticleSearchModel> cachedResult))
        {
            return cachedResult;
        }

        var results = new List<ArticleSearchModel>();
        
        foreach (var item in json.RootElement.EnumerateArray())
        {
            try
            {
                var model = new ArticleSearchModel
                {
                    DistributorId = item.GetProperty("distributorId").GetInt32(),
                    Grp = item.TryGetProperty("grp", out var grpProp) && grpProp.ValueKind != JsonValueKind.Null 
                        ? grpProp.GetString() 
                        : null,
                    Code = item.GetProperty("code").GetString() ?? string.Empty,
                    Brand = item.GetProperty("brand").GetString() ?? string.Empty,
                    Number = item.GetProperty("number").GetString() ?? string.Empty,
                    NumberFix = item.GetProperty("numberFix").GetString() ?? string.Empty,
                    Description = item.GetProperty("description").GetString() ?? string.Empty,
                    Availability = item.GetProperty("availability").GetInt32(),
                    Packing = item.GetProperty("packing").GetInt32(),
                    DeliveryPeriod = item.GetProperty("deliveryPeriod").GetInt32(),
                    DeliveryPeriodMax = item.GetProperty("deliveryPeriodMax").GetInt32(),
                    DeadlineReplace = item.GetProperty("deadlineReplace").GetString() ?? string.Empty,
                    DistributorCode = item.GetProperty("distributorCode").GetString() ?? string.Empty,
                    SupplierCode = item.GetProperty("supplierCode").GetInt32(),
                    SupplierColor = item.TryGetProperty("supplierColor", out var colorProp) && colorProp.ValueKind != JsonValueKind.Null 
                        ? colorProp.GetString() 
                        : null,
                    SupplierDescription = item.GetProperty("supplierDescription").GetString() ?? string.Empty,
                    ItemKey = item.GetProperty("itemKey").GetString() ?? string.Empty,
                    Price = item.GetProperty("price").GetDecimal(),
                    MaxPrice = item.GetProperty("maxPrice").GetDecimal(),
                    Weight = item.GetProperty("weight").GetDecimal(),
                    Volume = item.TryGetProperty("volume", out var volumeProp) && volumeProp.ValueKind != JsonValueKind.Null 
                        ? volumeProp.GetDecimal() 
                        : null,
                    LastUpdateTime = DateTime.Parse(item.GetProperty("lastUpdateTime").GetString() ?? string.Empty),
                    AdditionalPrice = item.GetProperty("additionalPrice").GetDecimal(),
                    NoReturn = item.GetProperty("noReturn").GetBoolean(),
                    IsUsed = item.GetProperty("isUsed").GetBoolean(),
                    DeliveryProbability = item.GetProperty("deliveryProbability").GetInt32(),
                    DescriptionOfDeliveryProbability = item.GetProperty("descriptionOfDeliveryProbability").GetString() ?? string.Empty,
                    PriceIn = item.GetProperty("priceIn").GetDecimal(),
                    PriceRate = item.GetProperty("priceRate").GetDecimal(),
                    IsAnalog = item.GetProperty("isAnalog").GetBoolean(),
                    Meta = new ArticleMetaModel
                    {
                        ProductId = item.GetProperty("meta").GetProperty("productId").GetInt32(),
                        Wearout = item.GetProperty("meta").GetProperty("wearout").GetInt32(),
                        IsUsed = item.GetProperty("meta").GetProperty("isUsed").GetBoolean(),
                        Images = item.GetProperty("meta").TryGetProperty("images", out var imagesProp) && imagesProp.ValueKind != JsonValueKind.Null 
                            ? imagesProp.EnumerateArray().Select(x => x.GetString()).ToArray() 
                            : null,
                        AbcpWh = item.GetProperty("meta").GetProperty("abcpWh").GetString() ?? string.Empty
                    }
                };
                
                results.Add(model);
            }
            catch (Exception ex)
            {
                // Логирование ошибки парсинга элемента
                Console.WriteLine($"Error parsing article item: {ex.Message}");
            }
        }

        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(30));

        _memoryCache.Set(cacheKey, results, cacheEntryOptions);
        
        return results;
    }
}