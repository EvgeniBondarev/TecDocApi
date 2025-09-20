using OzonOrdersWeb.Areas.PartsInfo.Models.ProductInformation;

namespace OzonOrdersWeb.Areas.PartsInfo.Models;

public class CombinedProductModel
{
    public ArticleFullModel DetailInfo { get; set; }
    public ProductInformationModel ProductInfo { get; set; }
    public int OrderId { get; set; }
    public string Article { get; set; }
    public string Manufacturer { get; set; }
    public List<string> ImageUrls { get; set; } = new List<string>();
}