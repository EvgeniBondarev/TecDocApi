using System.ComponentModel.DataAnnotations;

namespace OzonOrdersWeb.Areas.PartsInfo.Models.PrPart;

public class PrArticleFullModel
{
    public string Article { get; set; }
    public string Brand { get; set; }
    public string VendorCode { get; set; }
    public List<string> ImgUrls { get; set; }
    public List<VendorAttributeModel> DetailAttributes { get; set; }
    public string VendorCategoryName { get; set; }
    
    [Display(Name = "OEM")]
    public string OemCode { get; set; }
    
    [Display(Name = "Mark")]
    public string OemMark { get; set; }
    public List<string> Models { get; set; }
}
