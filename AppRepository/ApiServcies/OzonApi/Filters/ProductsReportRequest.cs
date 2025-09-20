using Newtonsoft.Json;

namespace Servcies.ApiServcies.OzonApi.Filters
{
    public class ProductsReportRequest : IRequestModel
    {
        [JsonProperty("language")]
        public string Language { get; set; }

        [JsonProperty("offer_id")]
        public string[] OfferId { get; set; } //Артикул

        [JsonProperty("search")]
        public string Search { get; set; }

        [JsonProperty("sku")]
        public string[] Sku { get; set; } //OZON id

        [JsonProperty("visibility")]
        public string Visibility { get; set; }
    }
}
