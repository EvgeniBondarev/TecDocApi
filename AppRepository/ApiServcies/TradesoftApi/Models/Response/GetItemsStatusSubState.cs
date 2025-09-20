using Newtonsoft.Json;

namespace Servcies.ApiServcies.TradesoftApi.Models.Response;

public class GetItemsStatusSubState
{
    [JsonProperty("innerId")]
    public string InnerId { get; set; }

    [JsonProperty("stateId")]
    public string StateId { get; set; }

    [JsonProperty("stateName")]
    public string StateName { get; set; }

    [JsonProperty("quantity")]
    public string Quantity { get; set; }
}