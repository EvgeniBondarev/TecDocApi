using Newtonsoft.Json;

namespace Servcies.ApiServcies.OzonApi.Filters;

public class ProductImportPricesRequest : IRequestModel
{
    [JsonProperty("prices")]
    public List<ProductPriceItem> Prices { get; set; } = new();
}