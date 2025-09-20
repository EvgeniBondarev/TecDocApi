using Newtonsoft.Json;

namespace Servcies.ApiServcies.TradesoftApi.Models.Response;

public class ProviderListResponse
{
    [JsonProperty("data")]
    public List<ProviderInfo> Data { get; set; }
    
    
    [JsonProperty("time")]
    public double Time { get; set; }
}