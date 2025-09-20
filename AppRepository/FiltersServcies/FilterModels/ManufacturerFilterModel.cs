using OServcies.FiltersServcies.FilterModels;

namespace Servcies.FiltersServcies.FilterModels
{
    public class ManufacturerFilterModel : ITableFilterModel
    {
        public string Code { get; set; }
        public string Name { get; set; }
    }
}
