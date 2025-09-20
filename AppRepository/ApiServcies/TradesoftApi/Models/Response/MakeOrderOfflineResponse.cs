using Newtonsoft.Json;

namespace Servcies.ApiServcies.TradesoftApi.Models.Response;

public class MakeOrderOfflineResponse
{
    [JsonProperty("error")]
    public string Error { get; set; }

    [JsonProperty("result")]
    public List<MakeOrderOfflineResult> Result { get; set; }

    [JsonProperty("time")]
    public double Time { get; set; }
}
