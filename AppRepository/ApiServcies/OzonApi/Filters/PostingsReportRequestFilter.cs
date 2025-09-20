using Newtonsoft.Json;

namespace Servcies.ApiServcies.OzonApi.Filters
{
    public class PostingsReportRequestFilter
    {
        [JsonProperty("processed_at_from")]
        public DateTime ProcessedAtFrom { get; set; }

        [JsonProperty("processed_at_to")]
        public DateTime ProcessedAtTo { get; set; }

        [JsonProperty("delivery_schema")]
        public string[] DeliverySchema { get; set; }

        [JsonProperty("sku")]
        public string[] Sku { get; set; }

        [JsonProperty("cancel_reason_id")]
        public string[] CancelReasonId { get; set; }

        [JsonProperty("offer_id")]
        public string OfferId { get; set; }

        [JsonProperty("status_alias")]
        public string[] StatusAlias { get; set; }

        [JsonProperty("statuses")]
        public string[] Statuses { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }
    }
}
