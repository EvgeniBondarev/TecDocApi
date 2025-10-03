using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace OzonDomains.Models
{
    public class Supplier
    {
        public int Id { get; set; }

        [StringLength(50)]
        [Display(Name = "Поставщик")]
        public string? Name { get; set; }

        [Display(Name = "Коэффицент себестоимости")]
        public decimal? CostFactor { get; set; } = 1;

        [Display(Name = "Стоимость за кг")]
        public decimal? WeightFactor { get; set; } = 1;

        [Display(Name = "Валюта")]
        public CurrencyCode CurrencyCode { get; set; }

        [Display(Name = "Валюта стоимость за кг")]
        public CurrencyCode WeightFactorCurrencyCode { get; set; }
        
        [NotMapped] 
        public string WeightFactorCurrencyDisplayName
        {
            get
            {
                var field = WeightFactorCurrencyCode.GetType().GetField(WeightFactorCurrencyCode.ToString());
                var displayAttribute = field?.GetCustomAttribute<DisplayAttribute>();
                return displayAttribute?.Name ?? WeightFactorCurrencyCode.ToString();
            }
        }

        [StringLength(500)]
        [Display(Name = "CSV URL")]
        public string? CsvUrl { get; set; }
        
        [StringLength(500)]
        [Display(Name = "Сайт")]
        public string? Site { get; set; }
        
        [Display(Name = "Дополнительный срок")]
        public int? AdditionalTerm { get; set; }
        
        [Display(Name = "Облагается НДС")]
        public bool IsVatApplicable { get; set; } = false;
    }
}
