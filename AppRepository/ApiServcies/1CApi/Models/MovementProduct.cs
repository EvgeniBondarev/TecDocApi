using Newtonsoft.Json;

public class MovementProduct
{
    [JsonProperty("Article")]
    public string Article { get; set; }
        
    [JsonProperty("quantity")]
    public int Quantity { get; set; }
}