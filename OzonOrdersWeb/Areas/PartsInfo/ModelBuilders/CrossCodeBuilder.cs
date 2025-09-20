using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using OzonOrdersWeb.Areas.PartsInfo.Models;
using OzonOrdersWeb.Areas.PartsInfo.Models.CrossCode;
using PartsInfo.HttpUtils;

namespace OzonOrdersWeb.Areas.PartsInfo.ModelBuilders;

public class CrossCodeBuilder
{
    private readonly IMemoryCache _memoryCache;
    
    public CrossCodeBuilder(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }
    
    public IEnumerable<CrossCodeModel> BuildModel(JsonDocument json)
    {
        foreach (var item in json.RootElement.EnumerateArray())
        {
            if (!item.TryGetProperty("cr_bycode", out var codeProp) || string.IsNullOrWhiteSpace(codeProp.GetString()))
                continue;

            if (!item.TryGetProperty("et_producer", out var producerProp))
                continue;

            yield return new CrossCodeModel
            {
                Code = codeProp.GetString(),
                Supplier = new SupplierModel
                {
                    Name = producerProp.TryGetProperty("name", out var nameProp) ? nameProp.GetString() ?? "" : "",
                    Rating = producerProp.TryGetProperty("rating", out var ratingProp) && ratingProp.TryGetInt32(out var ratingVal) ? ratingVal : 0,
                    ImageUrl = producerProp.TryGetProperty("img", out var imgProp) ? imgProp.GetString() ?? "" : "",
                    TecdocSupplierId = producerProp.TryGetProperty("tecdocSupplierId", out var tdIdProp) && tdIdProp.TryGetInt32(out var tdIdVal) ? tdIdVal : 0,
                    JSSupplierId = producerProp.TryGetProperty("id", out var idProp) && idProp.TryGetInt32(out var jsIdVal) ? jsIdVal : 0,
                    Code = producerProp.TryGetProperty("prefix", out var prefixProp) ? prefixProp.GetString() ?? "" : "", }
            };
        }
    }

}