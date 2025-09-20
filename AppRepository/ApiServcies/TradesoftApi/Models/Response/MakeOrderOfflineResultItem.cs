using Newtonsoft.Json;

namespace Servcies.ApiServcies.TradesoftApi.Models.Response;

public class MakeOrderOfflineResultItem
{
    [JsonProperty("itemId")]
    public string ItemId { get; set; }

    [JsonProperty("orderItemId")]
    public string OrderItemId { get; set; }

    [JsonProperty("error")]
    public string Error { get; set; }
}
