using OzonDomains.Models;

public class PdfService
{
    public byte[] GenerateTransactionPdf(Transaction transaction)
    {
        if (transaction == null) return null;

        var builder = PdfBuilderFactory.GetBuilder(transaction.Type);

        builder.BuildHeader(transaction);
        builder.BuildOrdersTable(transaction.Orders);

        return builder.GetPdf();
    }
}