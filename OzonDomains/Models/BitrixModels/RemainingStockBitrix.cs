using System.ComponentModel.DataAnnotations.Schema;
using OzonOrdersWeb.Areas.Studio2.ViewModels.Bitrix;
using OzonRepositories.Data.Bitrix;

namespace OzonDomains.Models.BitrixModels;

public class RemainingStockBitrix
{
    public int ProductId { get; set; }
    public string? Article { get; set; }
    public string? PreviewText { get; set; }
    public string Active { get; set; } = "Y";
    public string Available { get; set; } = "Y";
    public string? Supplier { get; set; }
    [NotMapped] public string? MarketPrefix { get; set; }
    public DateTime TimestampX { get; set; }
    public double Quantity { get; set; }
    public double? Price { get; set; }
    public string ActiveDisplay => Active == "Y" ? "Да" : "Нет";
    public string AvailableDisplay => Available == "Y" ? "Да" : "Нет";
    public string ArticleWithKey => $"{Article}={MarketPrefix}";
    [NotMapped] public PriceHistory? PriceHistory { get; set; }
    public List<StockByStore> Stores { get; set; } = new();
    public List<StockByStore> OzonStores { get; set; } = new();
    public List<OzonPriceIndexes> OzonPriceIndexes { get; set; } = new List<OzonPriceIndexes>();
}