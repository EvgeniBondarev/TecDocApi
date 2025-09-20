using Newtonsoft.Json;

public class ProductPriceMarketingAction
{
    [JsonProperty("title")]
    public string Title { get; set; }

    [JsonProperty("value")]
    public int Value { get; set; }

    [JsonProperty("date_from")]
    public DateTime DateFrom { get; set; }

    [JsonProperty("date_to")]
    public DateTime DateTo { get; set; }
}