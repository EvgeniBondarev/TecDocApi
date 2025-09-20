using Newtonsoft.Json;

public class ArticleMeta
{
    [JsonProperty("productId")]
    public long ProductId { get; set; }

    [JsonProperty("wearout")]
    public int Wearout { get; set; }

    [JsonProperty("isUsed")]
    public bool IsUsed { get; set; }

    [JsonProperty("images")]
    public string Images { get; set; }

    [JsonProperty("abcpWh")]
    public string AbcpWh { get; set; }
}