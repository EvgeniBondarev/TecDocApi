using Newtonsoft.Json;

namespace Servcies.ApiServcies.TradesoftApi.Models.Response;

public class GetItemsStatusProviderContainer
{
    [JsonProperty("provider")]
    public string Provider { get; set; }

    [JsonProperty("items")]
    public Dictionary<string, GetItemsStatusItem> Items { get; set; }

    [JsonProperty("time")]
    public double Time { get; set; }
}