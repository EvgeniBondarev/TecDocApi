using Newtonsoft.Json;

namespace Servcies.ApiServcies.AbcpApi.Models.Response;

public class DistributorShortInfo
{
    [JsonProperty("id")]
    public string Id { get; set; }
    
    [JsonProperty("name")]
    public string Name { get; set; }
}