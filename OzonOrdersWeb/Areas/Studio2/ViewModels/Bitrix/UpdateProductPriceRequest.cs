namespace OzonOrdersWeb.Areas.Studio2.Views;

public class UpdateProductPriceRequest
{
    public string Article { get; set; }
    public string ClientName { get; set; }
    public double ConstantIndex {get; set;}
    public double Percent {get; set;}
}
