using OzonDomains.Models;
using Servcies.FiltersServcies.FilterModels;

namespace Servcies.FiltersServcies.DataFilterManagers
{
    public class ColumnMappingDataFilterManager : IFilterManager<ColumnMapping, ColumnMappingFilterModel>
    {
        private readonly DataFilter<ColumnMapping> _filter;

        public ColumnMappingDataFilterManager(DataFilter<ColumnMapping> filter) 
        {
            _filter = filter;
        }

        public List<ColumnMapping> FilterByFilterData(List<ColumnMapping> standartColumnMappings, ColumnMappingFilterModel filterData)
        {
            List<ColumnMapping> columnMappings = _filter.FilterByString(standartColumnMappings, c => c.MappingName, filterData.MappingName).ToList();

            return columnMappings;
        }

        public List<ColumnMapping> FilterByRadioButton(List<ColumnMapping> filterModel, string buttonState)
        {
            throw new NotImplementedException();
        }
    }
}
