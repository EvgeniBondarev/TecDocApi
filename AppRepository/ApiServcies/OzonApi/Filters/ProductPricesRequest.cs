using Newtonsoft.Json;

namespace Servcies.ApiServcies.OzonApi.Filters
{
    public class ProductPricesRequest : IRequestModel
    {
        [JsonProperty("filter")]
        public ProductPricesRequestFilter Filter { get; set; }

        [JsonProperty("last_id")]
        public string LastId { get; set; }

        [JsonProperty("limit")]
        public int Limit { get; set; }
    }
}
