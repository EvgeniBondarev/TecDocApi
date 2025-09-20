using System.Text.Json.Serialization;

namespace OzonDomains.Models.OrderCarts.Cart;

public class StockItem
{
    [JsonPropertyName("orderId")]
    public int OrderId { get; set; }

    [JsonPropertyName("stokId")]
    public string StokId { get; set; }
}