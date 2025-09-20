using Newtonsoft.Json;

namespace Servcies.ApiServcies.OzonApi.Filters.ProductPriceses;

public class ProductPriceIndexes
{
    [JsonProperty("external_index_data")]
    public ProductPriceIndexData ExternalIndexData { get; set; }

    [JsonProperty("ozon_index_data")]
    public ProductPriceIndexData OzonIndexData { get; set; }

    [JsonProperty("color_index")]
    public string ColorIndex { get; set; }

    [JsonProperty("self_marketplaces_index_data")]
    public ProductPriceIndexData SelfMarketplacesIndexData { get; set; }
}
