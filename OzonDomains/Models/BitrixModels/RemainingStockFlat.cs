namespace OzonDomains.Models.BitrixModels;

public class RemainingStockFlat
{
    public int ProductId { get; set; }
    public string? Article { get; set; }
    public string? PreviewText { get; set; }
    public string Active { get; set; } = "Y";
    public string Available { get; set; } = "Y";
    public string? Supplier { get; set; }
    public DateTime TimestampX { get; set; }
    public double Quantity { get; set; }
    public double? Price { get; set; }

    public int StoreId { get; set; }
    public string StoreTitle { get; set; } = "";
    public double Amount { get; set; }
}
