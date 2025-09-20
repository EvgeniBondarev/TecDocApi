using Newtonsoft.Json;

namespace Servcies.ApiServcies.OzonApi.Filters
{
    public class ProductPricesRequestFilter
    {
        [JsonProperty("offer_id")]
        public string[] OfferId { get; set; }

        [JsonProperty("product_id")]
        public string[] ProductId { get; set; }

        [JsonProperty("visibility")]
        public string Visibility { get; set; }
    }
}
