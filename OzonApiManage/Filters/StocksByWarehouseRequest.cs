using Newtonsoft.Json;

namespace OzonApiManage.Filters
{
    public class StocksByWarehouseRequest : IRequestModel
    {
        [JsonProperty("sku")]
        public string[] Sku { get; set; }

        [JsonProperty("fbs_sku")]
        public string[] FbsSku { get; set; }
    }
}
