using Newtonsoft.Json;

namespace Servcies.ApiServcies.TradesoftApi.Models;

public class PreOrderContainer
{
    [JsonProperty("provider")]
    public string Provider { get; set; }
    
    [JsonProperty("login")]
    public string Login { get; set; }
    
    [JsonProperty("password")]
    public string Password { get; set; }
    
    [JsonProperty("code")]
    public string Code { get; set; }
    
    [JsonProperty("producer")]
    public string Producer { get; set; }
    
    [JsonProperty("itemHash")]
    public string ItemHash { get; set; }
}