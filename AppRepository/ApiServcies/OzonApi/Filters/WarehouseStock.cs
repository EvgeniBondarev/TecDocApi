using Newtonsoft.Json;

namespace Servcies.ApiServcies.OzonApi.Filters;

public class WarehouseStock
{
    [JsonProperty("product_id")]
    public long product_id { get; set; }

    [JsonProperty("present")]
    public int present { get; set; }

    [JsonProperty("reserved")]
    public int reserved { get; set; }

    [JsonProperty("sku")]
    public long sku { get; set; }

    [JsonProperty("warehouse_id")]
    public long warehouse_id { get; set; }

    [JsonProperty("warehouse_name")]
    public string warehouse_name { get; set; }

    [JsonProperty("fbs_sku")]
    public long fbs_sku { get; set; }
}