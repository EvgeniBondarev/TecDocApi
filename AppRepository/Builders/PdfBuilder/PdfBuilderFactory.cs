using OzonDomains;
using Servcies.Builders.PdfBuilder.Builders;

public static class PdfBuilderFactory
{
    public static IPdfBuilder GetBuilder(TransactionType? type)
    {
        return type switch
        {
            TransactionType.ShippedToSeller => new ShippedToSellerPdfBuilder(),
            TransactionType.ShippedBySupplier => new ShippedBySupplierPdfBuilder(),
            _ => new ShippedToSellerPdfBuilder() 
        };
    }
}