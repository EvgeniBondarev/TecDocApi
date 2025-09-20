using Newtonsoft.Json;

namespace Servcies.ApiServcies.OzonApi.Filters
{
    public class StocksByWarehouseRequest : IRequestModel
    {
        [JsonProperty("sku")]
        public string[] Sku { get; set; }

        [JsonProperty("fbs_sku")]
        public string[] FbsSku { get; set; }
    }
}
