using Newtonsoft.Json;

namespace Servcies.ApiServcies.TradesoftApi.Models;

public class PreOrderSearchRequest : IRequestModel
{
    [JsonProperty("service")]
    public string Service { get; set; } = "provider";
    
    [JsonProperty("action")]
    public string Action { get; set; } = "PreOrderSearch";
    
    [JsonProperty("user")]
    public string User { get; set; }
    
    [JsonProperty("password")]
    public string Password { get; set; }
    
    [JsonProperty("timeLimit")]
    public string TimeLimit { get; set; } = "10"; 
    
    [JsonProperty("container")]
    public List<PreOrderContainer> Container { get; set; }
}