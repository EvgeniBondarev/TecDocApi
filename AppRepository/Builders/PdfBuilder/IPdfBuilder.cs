using OzonDomains.Models;
using OzonDomains.Models.BitrixModels;

public interface IPdfBuilder
{
    void BuildHeader(Transaction transaction);
    void BuildOrdersTable(ICollection<Order> orders);
    void SetOrdersStock(Dictionary<int, List<StockInfo>> stocks);
    byte[] GetPdf();
}