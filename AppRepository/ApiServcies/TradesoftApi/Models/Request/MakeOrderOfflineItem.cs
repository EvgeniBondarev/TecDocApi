using Newtonsoft.Json;
using Servcies.ApiServcies;

public class MakeOrderOfflineItem
{
    [JsonProperty("itemId")]
    public string ItemId { get; set; }

    [JsonProperty("quantity")]
    public string Quantity { get; set; }

    [JsonProperty("reference")]
    public string Reference { get; set; }

    [JsonProperty("comment")]
    public string Comment { get; set; }
}