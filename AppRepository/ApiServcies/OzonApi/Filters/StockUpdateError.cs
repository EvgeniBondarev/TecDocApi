using Newtonsoft.Json;

namespace Servcies.ApiServcies.OzonApi.Filters;

public class StockUpdateError
{
    [JsonProperty("code")]
    public string Code { get; set; }

    [JsonProperty("message")]
    public string Message { get; set; }

    [JsonProperty("offer_id")]
    public string OfferId { get; set; }

    [JsonProperty("product_id")]
    public long ProductId { get; set; }

    [JsonProperty("warehouse_id")]
    public long WarehouseId { get; set; }
}