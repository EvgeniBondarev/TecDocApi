using System.ComponentModel.DataAnnotations;
using OzonOrdersWeb.Areas.PartsInfo.Models;
using OzonOrdersWeb.Areas.PartsInfo.Models.FullInfo;
using OzonOrdersWeb.Areas.PartsInfo.Models.PrPart;

public class ArticleFullModel
{
    [Display(Name = "Нормализованный артикул")]
    public string NormalizedArticle { get; set; }
    public ArticleEanModel ArticleEan { get; set; }
    public ArticleSchemaModel ArticleSchema { get; set; }
    public List<DetailAttributeModel> DetailAttributes { get; set; }
    public List<string> ImgUrls { get; set; }
    public SupplierModel Supplier { get; set; }
    public string Description { get; set; }
    
    public PrArticleFullModel? PrData { get; set; }
}