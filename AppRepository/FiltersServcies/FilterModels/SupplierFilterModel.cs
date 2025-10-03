using OServcies.FiltersServcies.FilterModels;
using OzonDomains;

namespace Servcies.FiltersServcies.FilterModels
{
    public class SupplierFilterModel : ITableFilterModel
    {
        public string? Name { get; set; }
        public decimal? CostFactor { get; set; }
        public decimal? WeightFactor { get; set; }
        private CurrencyCode? _currencyCode { get; set; }
        public CurrencyCode? CurrencyCode
        {
            get { return _currencyCode; }
            set
            {
                if (value == OzonDomains.CurrencyCode.NON)
                {
                    _currencyCode = null;
                }
                else
                {
                    _currencyCode = value;
                }
            }
        }

        private CurrencyCode? _weightFactorCurrencyCode { get; set; }
        public CurrencyCode? WeightFactorCurrencyCode
        {
            get { return _weightFactorCurrencyCode; }
            set
            {
                if (value == OzonDomains.CurrencyCode.NON)
                {
                    _weightFactorCurrencyCode = null;
                }
                else
                {
                    _weightFactorCurrencyCode = value;
                }
            }
        }
        
        public string? CsvUrl { get; set; }
        public string? Site { get; set; }
        public int? AdditionalTerm { get; set; }
        public bool? IsVatApplicable { get; set; }
        public string? INNCode { get; set; }
    }
}
