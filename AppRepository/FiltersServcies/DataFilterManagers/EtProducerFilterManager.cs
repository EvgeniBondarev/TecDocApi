using Microsoft.EntityFrameworkCore;
using OzonDomains.Models;
using Servcies.DataServcies;
using Servcies.FiltersServcies.FilterModels;

namespace Servcies.FiltersServcies.DataFilterManagers;

public class EtProducerFilterManager : IFilterManager<EtProducer, EtProducerFilterModel>
{
    private readonly DataFilter<EtProducer> _filter;
    private readonly QueryableDataFilter<EtProducer> _queryableFilter;
    private readonly EtProducerDataServices _dataService;
    public EtProducerFilterManager(DataFilter<EtProducer> dataFilter, 
                                    QueryableDataFilter<EtProducer> queryableFilter,
                                    EtProducerDataServices dataService)
    {
        _filter = dataFilter;
        _queryableFilter = queryableFilter;
        _dataService = dataService;
    }

    public List<EtProducer> FilterByFilterData(List<EtProducer> producers, EtProducerFilterModel filterData)
    {
        producers = _filter.FilterByString(producers, pr => pr.Prefix, filterData.Prefix).ToList();
        producers = _filter.FilterByString(producers, pr => pr.Name, filterData.Name).ToList();
        producers = _filter.FilterByString(producers, pr => pr.Address, filterData.Address).ToList();
        producers = _filter.FilterByString(producers, pr => pr.Domain, filterData.Domain).ToList();
        producers = _filter.FilterByString(producers, pr => pr.Www, filterData.Www).ToList();
        producers = _filter.FilterByInt(producers, pr => pr.TecdocSupplierId, filterData.TecdocSupplierId).ToList();
        producers = _filter.FilterByString(producers, pr => pr.MarketPrefix, filterData.MarketPrefix).ToList();
        producers = _filter.FilterByInt(producers, pr => pr.Rating, filterData.Rating).ToList();
        producers = _filter.FilterByString(producers, pr => pr.ExistName, filterData.ExistName).ToList();

        return producers;
    }
    
    public async Task<List<EtProducer>> FilterByFilterDataAsync(EtProducerFilterModel filterData)
    {
        var query = _dataService.GetQueryableEtProducers();

        if (!string.IsNullOrEmpty(filterData.Prefix))
            query = _queryableFilter.FilterByString(query, pr => pr.Prefix, filterData.Prefix);

        if (!string.IsNullOrEmpty(filterData.Name))
            query = _queryableFilter.FilterByString(query, pr => pr.Name, filterData.Name);

        if (!string.IsNullOrEmpty(filterData.Address))
            query = _queryableFilter.FilterByString(query, pr => pr.Address, filterData.Address);

        if (!string.IsNullOrEmpty(filterData.Domain))
            query = _queryableFilter.FilterByString(query, pr => pr.Domain, filterData.Domain);

        if (!string.IsNullOrEmpty(filterData.Www))
            query = _queryableFilter.FilterByString(query, pr => pr.Www, filterData.Www);

        if (filterData.TecdocSupplierId != 0)
            query = _queryableFilter.FilterByInt(query, pr => pr.TecdocSupplierId, filterData.TecdocSupplierId);

        if (!string.IsNullOrEmpty(filterData.MarketPrefix))
            query = _queryableFilter.FilterByString(query, pr => pr.MarketPrefix, filterData.MarketPrefix);

        if (filterData.Rating != 0)
            query = _queryableFilter.FilterByInt(query, pr => pr.Rating, filterData.Rating);
        
        if (!string.IsNullOrEmpty(filterData.ExistName))
            query = _queryableFilter.FilterByString(query, pr => pr.ExistName, filterData.ExistName);

        if (!string.IsNullOrEmpty(filterData.Id))
            query = _queryableFilter.FilterByInt(query, pr => pr.Id, int.Parse(filterData.Id));

        if (filterData.RealId != 0)
            query = _queryableFilter.FilterByInt(query, pr =>pr.RealId, filterData.RealId); 

        return await query.ToListAsync();
    }

    public List<EtProducer> FilterByRadioButton(List<EtProducer> filterModel, string buttonState)
    {
        throw new NotImplementedException();
    }
}