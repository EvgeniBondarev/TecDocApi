using Newtonsoft.Json;
using Servcies.ApiServcies.OzonApi.Filters.ProductPriceses;

public class ProductPriceItemResponse
{
    [JsonProperty("acquiring")]
    public double Acquiring { get; set; }

    [JsonProperty("offer_id")]
    public string OfferId { get; set; }

    [JsonProperty("product_id")]
    public long ProductId { get; set; }

    [JsonProperty("volume_weight")]
    public double VolumeWeight { get; set; }

    [JsonProperty("commissions")]
    public ProductPriceCommissions Commissions { get; set; }

    [JsonProperty("marketing_actions")]
    public ProductPriceMarketingActions MarketingActions { get; set; }

    [JsonProperty("price")]
    public ProductPriceDetail Price { get; set; }

    [JsonProperty("price_indexes")]
    public ProductPriceIndexes PriceIndexes { get; set; }
}