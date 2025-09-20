using System.ComponentModel.DataAnnotations;

namespace OzonOrdersWeb.Areas.PartsInfo.Models.ABCP;

public class TipModel
{
    [Display(Name = "Бренд")]
    public string Brand { get; set; }

    [Display(Name = "Номер")]
    public string Number { get; set; }

    [Display(Name = "Описание")]
    public string Description { get; set; }
}
