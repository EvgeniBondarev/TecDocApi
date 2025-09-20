using Newtonsoft.Json;

public class StockItemForUpdate
{
    [JsonProperty("offer_id")]
    public string OfferId { get; set; }

    [JsonProperty("product_id")]
    public long ProductId { get; set; }

    [JsonProperty("stock")]
    public int Stock { get; set; }

    [JsonProperty("warehouse_id")]
    public long WarehouseId { get; set; }
}