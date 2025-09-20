using Newtonsoft.Json;

namespace Servcies.ApiServcies.TradesoftApi.Models;

public class ProviderListRequest : IRequestModel
{
    [JsonProperty("service")]
    public string Service { get; set; } = "provider";
    
    [JsonProperty("action")]
    public string Action { get; set; } = "GetProviderList";
    
    [JsonProperty("user")]
    public string User { get; set; }
    
    [JsonProperty("password")]
    public string Password { get; set; }
}
