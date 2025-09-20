using Newtonsoft.Json;

namespace Servcies.ApiServcies.TradesoftApi.Models;

public class MakeOrderOfflineRequest : IRequestModel
{
    [JsonProperty("user")]
    public string User { get; set; }

    [JsonProperty("password")]
    public string Password { get; set; }

    [JsonProperty("service")]
    public string Service { get; set; } = "provider";

    [JsonProperty("action")]
    public string Action { get; set; } = "makeOrderOffline";

    [JsonProperty("timeLimit", NullValueHandling = NullValueHandling.Ignore)]
    public int? TimeLimit { get; set; } // Необязательный параметр

    [JsonProperty("param")]
    public List<MakeOrderOfflineParam> Param { get; set; }
}
