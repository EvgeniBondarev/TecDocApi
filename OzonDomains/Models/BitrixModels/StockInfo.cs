namespace OzonDomains.Models.BitrixModels;

// DTO класс для информации о складе
public class StockInfo
{
    public int StoreId { get; set; }
    public string StoreTitle { get; set; }
    public double Amount { get; set; }
}