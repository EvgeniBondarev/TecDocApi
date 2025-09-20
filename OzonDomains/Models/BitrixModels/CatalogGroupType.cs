using System.ComponentModel.DataAnnotations;

namespace OzonDomains.Models.BitrixModels;

public enum CatalogGroupType
{
    Unknown = 0,

    [Display(Name = "Розничная цена")]
    Retail = 1,

    [Display(Name = "Учетная цена")]
    Accounting = 3,

    [Display(Name = "Sale")]
    Sale = 4
}