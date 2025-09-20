using Newtonsoft.Json;

namespace Servcies.ApiServcies.OzonApi.Filters
{
    public class ProductAnalyticsRequest : IRequestModel
    {
        [JsonProperty("posting_number")]
        public string PostingNumber { get; set; }

        [JsonProperty("with")]
        public ProductAnalyticsRequestFilter With { get; set; }
    }
}
