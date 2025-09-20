using Newtonsoft.Json;

namespace Servcies.ApiServcies.OzonApi.Filters.ProductPriceses;


public class ProductPriceIndexData
{
    [JsonProperty("min_price")]
    public double MinPrice { get; set; }

    [JsonProperty("min_price_currency")]
    public string MinPriceCurrency { get; set; }

    [JsonProperty("price_index_value")]
    public double PriceIndexValue { get; set; }
}