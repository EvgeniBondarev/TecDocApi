using Newtonsoft.Json;

namespace Servcies.ApiServcies.YandexApi.Filters
{
    public class OrdersRequest : IRequestModel
    {
        [JsonProperty("buyerType")]
        public string BuyerType { get; set; }

        [JsonProperty("dispatchType")]
        public string DispatchType { get; set; }

        [JsonProperty("fake")]
        public bool? Fake { get; set; }

        [JsonProperty("fromDate")]
        public string FromDate { get; set; }

        [JsonProperty("toDate")]
        public string ToDate { get; set; }

        [JsonProperty("hasCis")]
        public bool? HasCis { get; set; }

        [JsonProperty("limit")]
        public int? Limit { get; set; }

        [JsonProperty("onlyEstimatedDelivery")]
        public bool? OnlyEstimatedDelivery { get; set; }

        [JsonProperty("onlyWaitingForCancellationApprove")]
        public bool? OnlyWaitingForCancellationApprove { get; set; }

        [JsonProperty("orderIds")]
        public long[] OrderIds { get; set; }

        [JsonProperty("page")]
        public int? Page { get; set; }

        [JsonProperty("pageSize")]
        public int? PageSize { get; set; }

        [JsonProperty("page_token")]
        public string PageToken { get; set; }

        [JsonProperty("status")]
        public string[] Status { get; set; }

        [JsonProperty("substatus")]
        public string[] Substatus { get; set; }

        [JsonProperty("supplierShipmentDateFrom")]
        public string SupplierShipmentDateFrom { get; set; }

        [JsonProperty("supplierShipmentDateTo")]
        public string SupplierShipmentDateTo { get; set; }

        [JsonProperty("updatedAtFrom")]
        public string UpdatedAtFrom { get; set; }

        [JsonProperty("updatedAtTo")]
        public string UpdatedAtTo { get; set; }
    }

}
