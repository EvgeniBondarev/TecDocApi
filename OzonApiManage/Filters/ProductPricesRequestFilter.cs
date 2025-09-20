using Newtonsoft.Json;

namespace OzonApiManage.Filters
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
