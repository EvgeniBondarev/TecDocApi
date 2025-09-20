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
            suppliers = _filter.FilterByString(suppliers, pr => pr.Name, filterData.Name).ToList();
            suppliers = _filter.FilterByDecimal(suppliers, pr => pr.CostFactor, filterData.CostFactor).ToList();
            suppliers = _filter.FilterByDecimal(suppliers, pr => pr.WeightFactor, filterData.WeightFactor).ToList();    
            suppliers = _filter.FilterByEnum(suppliers, pr => pr.CurrencyCode, filterData.CurrencyCode).ToList();
            suppliers = _filter.FilterByEnum(suppliers, pr => pr.WeightFactorCurrencyCode, filterData.WeightFactorCurrencyCode).ToList();
            suppliers = _filter.FilterByString(suppliers, pr => pr.CsvUrl, filterData.CsvUrl).ToList();
            suppliers = _filter.FilterByString(suppliers, pr => pr.Site, filterData.Site).ToList();
            suppliers = _filter.FilterByInt(suppliers, pr => pr.AdditionalTerm.Value, filterData.AdditionalTerm).ToList();

            return suppliers;
        }

        public List<Supplier> FilterByRadioButton(List<Supplier> filterModel, string buttonState)
        {
            throw new NotImplementedException();
        }
    }
}
