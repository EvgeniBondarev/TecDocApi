using System.ComponentModel.DataAnnotations;
using OzonOrdersWeb.Areas.PartsInfo.Models.PrPart;

namespace OzonOrdersWeb.Areas.PartsInfo.Models;

public class SupplierModel
{
    [Display(Name = "Название")]
    public string Name { get; set; }

    [Display(Name = "Рейтинг")]
    public int Rating { get; set; }

    [Display(Name = "Изображение")]
    public string ImageUrl { get; set; }

    [Display(Name = "Описание")]
    public string Description { get; set; }

    [Display(Name = "Tecdoc ID")]
    public int TecdocSupplierId { get; set; }

    [Display(Name = "JS ID")]
    public int JSSupplierId { get; set; }

    [Display(Name = "Префикс")]
    public string Code { get; set; }

    [Display(Name = "Источник изображения")]
    public ImageSource ImageSource { get; set; }
    
    public PrArticleFullModel? PrData { get; set; }
}