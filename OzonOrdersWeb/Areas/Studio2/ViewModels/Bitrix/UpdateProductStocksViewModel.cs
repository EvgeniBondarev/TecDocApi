namespace OzonOrdersWeb.Areas.Studio2.ViewModels.Bitrix;

public class UpdateProductStocksViewModel
{
    public string OzonClient { get; set; }
    public string OfferId { get; set; }
    public long ProductId { get; set; }
    public int Stock { get; set; }
    public long WarehouseId { get; set; }
}