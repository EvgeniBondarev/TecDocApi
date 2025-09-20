namespace OzonDomains.Models.BitrixModels;

public class StockByStore
{
    public int StoreId { get; set; }
    public string Title { get; set; } = string.Empty;
    public double Amount { get; set; }
    public string? ClientName { get; set; } = string.Empty;
}