using Newtonsoft.Json;

namespace Servcies.ApiServcies.TradesoftApi.Models.Response;

public class GetItemsStatusItem
{
    [JsonProperty("providerItemId")]
    public string ProviderItemId { get; set; }

    [JsonProperty("providerOrderNumber")]
    public string ProviderOrderNumber { get; set; }

    [JsonProperty("quantity")]
    public string Quantity { get; set; }

    [JsonProperty("price")]
    public string Price { get; set; }

    [JsonProperty("currency")]
    public string Currency { get; set; }

    [JsonProperty("stateId")]
    public string StateId { get; set; }

    [JsonProperty("stateName")]
    public string StateName { get; set; }

    [JsonProperty("error")]
    public string Error { get; set; }

    [JsonProperty("subStates")]
    public List<GetItemsStatusSubState> SubStates { get; set; }
}
