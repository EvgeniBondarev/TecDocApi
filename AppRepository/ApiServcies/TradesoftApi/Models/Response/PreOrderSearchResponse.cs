using Newtonsoft.Json;

namespace Servcies.ApiServcies.TradesoftApi.Models.Response;

public class PreOrderSearchResponse
{
    [JsonProperty("error")]
    public string Error { get; set; }

    [JsonProperty("container")]
    public List<PreOrderContainerResponse> Container { get; set; }

    [JsonProperty("time")]
    public double Time { get; set; }
}