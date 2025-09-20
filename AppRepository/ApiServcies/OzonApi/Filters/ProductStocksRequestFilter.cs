

using Newtonsoft.Json;

public class ProductStocksRequestFilter
{
    [JsonProperty("offer_id")]
    public string[] OfferId { get; set; }
}