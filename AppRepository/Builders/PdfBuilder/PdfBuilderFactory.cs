using OzonDomains;

public static class PdfBuilderFactory
{
    public static IPdfBuilder GetBuilder(TransactionType? type)
    {
        return type switch
        {
            TransactionType.OrderedToSupplier => new OrderedToSupplierPdfBuilder(),
            _ => new OrderedToSupplierPdfBuilder() 
        };
    }
}