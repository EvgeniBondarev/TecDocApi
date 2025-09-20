

using Newtonsoft.Json;
using Servcies.ApiServcies;

public class GetItemsStatusRequest : IRequestModel
{
    [JsonProperty("user")]
    public string User { get; set; }

    [JsonProperty("password")]
    public string Password { get; set; }

    [JsonProperty("service")]
    public string Service { get; set; } = "provider";

    [JsonProperty("action")]
    public string Action { get; set; } = "getItemsStatus";

    [JsonProperty("timeLimit", NullValueHandling = NullValueHandling.Ignore)]
    public int? TimeLimit { get; set; }

    [JsonProperty("container")]
    public List<GetItemsStatusContainer> Container { get; set; }
}
