namespace OzonOrdersWeb.Areas.Studio2.ViewModels.Bitrix;

public class SetProductPriceRequest
{
    public string Article { get; set; }
    public string ClientName { get; set; }
    public double YourPrice { get; set; }
    public double OldPrice { get; set; }
    public double MinPrice { get; set; }
    public double CostPrice { get; set; }
}