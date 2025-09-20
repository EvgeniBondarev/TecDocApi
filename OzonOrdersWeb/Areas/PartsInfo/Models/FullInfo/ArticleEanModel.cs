using System.ComponentModel.DataAnnotations;

namespace OzonOrdersWeb.Areas.PartsInfo.Models.FullInfo;

public class ArticleEanModel
{
    public int SupplierId { get; set; }
    public string DataSupplierArticleNumber { get; set; }
    [Display(Name = "Ean")]
    public string Ean { get; set; }
}