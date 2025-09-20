using Newtonsoft.Json;

namespace Servcies.ApiServcies.TradesoftApi.Models.Response;

public class PreOrderContainerResponse
{
    [JsonProperty("cache")]
    public string Cache { get; set; }
    
    [JsonProperty("provider")]
    public string Provider { get; set; }

    [JsonProperty("items")]
    public List<PreOrderItem> Items { get; set; }
}