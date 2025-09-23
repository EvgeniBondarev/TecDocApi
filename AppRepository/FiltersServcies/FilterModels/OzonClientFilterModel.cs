using OServcies.FiltersServcies.FilterModels;
using OzonDomains;

namespace Servcies.FiltersServcies.FilterModels
{
    public class OzonClientFilterModel : ITableFilterModel
    {
        public string? Name { get; set; }
        public string? INNCode { get; set; }
        public string? WarehouseName { get; set; }

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
    }
}
