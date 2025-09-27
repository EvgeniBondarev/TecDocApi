using OzonDomains;

public static class PdfBuilderFactory
{
    public static IPdfBuilder GetBuilder(TransactionType? type)
    {
        return type switch
        {
            TransactionType.ShippedToSeller => new ShippedToSellerPdfBuilder(),
            _ => new ShippedToSellerPdfBuilder() 
        };
    }
}