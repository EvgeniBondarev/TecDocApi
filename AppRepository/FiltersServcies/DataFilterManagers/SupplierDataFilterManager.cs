using OzonDomains.Models;
using Servcies.FiltersServcies.FilterModels;

namespace Servcies.FiltersServcies.DataFilterManagers
{
    public class SupplierDataFilterManager : IFilterManager<Supplier, SupplierFilterModel>
    {
        private readonly DataFilter<Supplier> _filter;

        public SupplierDataFilterManager(DataFilter<Supplier> filter)
        {
            _filter = filter;
        }

        public List<Supplier> FilterByFilterData(List<Supplier> suppliers, SupplierFilterModel filterData)
        {
            if (suppliers == null || filterData == null)
                return suppliers;

            suppliers = _filter.FilterByString(suppliers, pr => pr.Name, filterData.Name).ToList();
            suppliers = _filter.FilterByDecimal(suppliers, pr => pr.CostFactor, filterData.CostFactor).ToList();
            suppliers = _filter.FilterByDecimal(suppliers, pr => pr.WeightFactor, filterData.WeightFactor).ToList();
            suppliers = _filter.FilterByEnum(suppliers, pr => pr.CurrencyCode, filterData.CurrencyCode).ToList();
            suppliers = _filter.FilterByEnum(suppliers, pr => pr.WeightFactorCurrencyCode, filterData.WeightFactorCurrencyCode).ToList();
            suppliers = _filter.FilterByString(suppliers, pr => pr.CsvUrl, filterData.CsvUrl).ToList();
            suppliers = _filter.FilterByString(suppliers, pr => pr.Site, filterData.Site).ToList();
            suppliers = _filter.FilterByString(suppliers, pr => pr.INNCode, filterData.INNCode).ToList();
            
            // Безопасная фильтрация по nullable int
            if (filterData.AdditionalTerm.HasValue)
            {
                suppliers = suppliers.Where(pr => pr.AdditionalTerm.HasValue && pr.AdditionalTerm.Value == filterData.AdditionalTerm.Value).ToList();
            }

            // Фильтрация по nullable bool
            if (filterData.IsVatApplicable.HasValue)
            {
                suppliers = suppliers.Where(pr => pr.IsVatApplicable == filterData.IsVatApplicable.Value).ToList();
            }

            return suppliers;
        }

        public List<Supplier> FilterByRadioButton(List<Supplier> filterModel, string buttonState)
        {
            throw new NotImplementedException();
        }
    }
}
