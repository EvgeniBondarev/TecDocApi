using Newtonsoft.Json;
using Servcies.ApiServcies.OzonApi.Filters;

public class ProductImportPricesResponse
{
    [JsonProperty("result")]
    public List<ProductImportPriceResult> Result { get; set; } = new();
}