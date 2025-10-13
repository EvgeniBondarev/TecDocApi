namespace OzonOrdersWeb.Areas.Studio2.ViewModels.Bitrix;

public class OzonPriceIndexes
{
    public PriceIndexData ExternalIndex { get; set; }
    public PriceIndexData OzonIndex { get; set; }
    public PriceIndexData SelfMarketplacesIndex { get; set; }
    public string ColorIndex { get; set; }
    public string ClientName { get; set; }
}