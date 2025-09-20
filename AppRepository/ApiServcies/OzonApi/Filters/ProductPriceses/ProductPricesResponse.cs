using Newtonsoft.Json;

public class ProductPricesResponse
{
    [JsonProperty("items")]
    public List<ProductPriceItemResponse> Items { get; set; }

    [JsonProperty("cursor")]
    public string Cursor { get; set; }

    [JsonProperty("total")]
    public int Total { get; set; }
}