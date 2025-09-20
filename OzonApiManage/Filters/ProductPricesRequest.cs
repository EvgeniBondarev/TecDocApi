using Newtonsoft.Json;

namespace OzonApiManage.Filters
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
