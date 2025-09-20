using OzonDomains.Models;
using Servcies.FiltersServcies.FilterModels;

namespace Servcies.FiltersServcies.DataFilterManagers
{
    public class OzonClientDataFilterManager : IFilterManager<OzonClient, OzonClientFilterModel>
    {
        private readonly DataFilter<OzonClient> _filter;

        public OzonClientDataFilterManager(DataFilter<OzonClient> filter)
        {
            _filter = filter;
        }

        public List<OzonClient> FilterByFilterData(List<OzonClient> ozonClients, OzonClientFilterModel filterData)
        {
            ozonClients = _filter.FilterByString(ozonClients, oc => oc.Name, filterData.Name).ToList();
            ozonClients = _filter.FilterByEnum(ozonClients, oc => oc.CurrencyCode, filterData.CurrencyCode).ToList();

            return ozonClients;
        }

        public List<OzonClient> FilterByRadioButton(List<OzonClient> ozonClients, string buttonState)
        {
            throw new NotImplementedException();
        }
    }
}
