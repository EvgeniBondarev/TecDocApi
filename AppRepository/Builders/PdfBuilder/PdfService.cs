using OzonDomains.Models;
using OzonDomains.Models.BitrixModels;

public class PdfService
{
    public Dictionary<int, List<StockInfo>>? stocks;
    public byte[] GenerateTransactionPdf(Transaction transaction)
    {
        if (transaction == null) return null;

        var builder = PdfBuilderFactory.GetBuilder(transaction.Type);

        builder.BuildHeader(transaction);
        if (stocks != null)
        {
            builder.SetOrdersStock(stocks);
        }
        builder.BuildOrdersTable(transaction.Orders);

        return builder.GetPdf();
    }
}