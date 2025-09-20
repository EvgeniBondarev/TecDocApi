using Newtonsoft.Json;

namespace Servcies.ApiServcies.OzonApi.Filters;

public class UpdateStockResult
{
    [JsonProperty("warehouse_id")]
    public long WarehouseId { get; set; }

    [JsonProperty("product_id")]
    public long ProductId { get; set; }

    [JsonProperty("offer_id")]
    public string OfferId { get; set; }

    [JsonProperty("updated")]
    public bool Updated { get; set; }

    [JsonProperty("errors")]
    public List<StockUpdateError> Errors { get; set; }
}