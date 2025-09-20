using Newtonsoft.Json;

namespace Servcies.ApiServcies.OzonApi.Filters
{
    public class OrdersLableRequest : IRequestModel
    {
        [JsonProperty("posting_number")]
        public string[] ShipmentNumber { get; set; }
    }
}
