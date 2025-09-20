using System.ComponentModel.DataAnnotations;

namespace OzonDomains.Models
{
    public class Product
    {
        public int Id { get; set; }

        [MaxLength(255)]
        [Display(Name = "Артикул")]
        public string? Article { get; set; }

        [MaxLength(50)]
        [Display(Name = "Ozon Product ID")]
        public string? OzonProductId { get; set; }

        [MaxLength(50)]
        [Display(Name = "FBO OZON SKU ID")]
        public string? FboOzonSkuId { get; set; }

        [MaxLength(50)]
        [Display(Name = "FBS OZON SKU ID")]
        public string? FbsOzonSkuId { get; set; }

        [MaxLength(255)]
        [Display(Name = "Коммерческая категория")]
        public string? CommercialCategory { get; set; }


        [Display(Name = "Объем товара, л")]
        public double? Volume { get; set; }


        [Display(Name = "Объемный вес, кг")]
        public double? VolumetricWeight { get; set; }

        [Display(Name = "Вес, кг")]
        public decimal? Weight { get; set; }

        [Display(Name = "Текущая цена с учетом скидки, руб.")]
        public decimal? CurrentPriceWithDiscount { get; set; }
    }
}
