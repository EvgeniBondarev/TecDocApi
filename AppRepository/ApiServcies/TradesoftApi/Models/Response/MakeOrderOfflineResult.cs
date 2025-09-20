using Newtonsoft.Json;
using Servcies.ApiServcies.TradesoftApi.Models.Response;

public class MakeOrderOfflineResult
{
    [JsonProperty("provider")]
    public string Provider { get; set; }

    [JsonProperty("orderStatus")]
    public string OrderStatus { get; set; }

    [JsonProperty("items")]
    public List<MakeOrderOfflineResultItem> Items { get; set; }
}