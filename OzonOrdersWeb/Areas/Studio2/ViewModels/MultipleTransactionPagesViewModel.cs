namespace OzonOrdersWeb.Areas.Studio2.ViewModels;

public class MultipleTransactionPagesViewModel
{
    public List<WarehouseTransactionPage> WarehousePages { get; set; } = new();
}

public class WarehouseTransactionPage
{
    public string WarehouseName { get; set; }
    public string Url { get; set; }
}
