using OzonDomains.Models;
using OzonRepositories.Data;
namespace Servcies.DataServcies;

public class EtProducerDataServices : IDataServcies
{
    private readonly EtProducerRepository _repository;

    public EtProducerDataServices(EtProducerRepository repository)
    {
        _repository = repository;
    }

    public Task<int> AddEtProducer(EtProducer value)
    {
        return _repository.Add(value);
    }
    
    public Task<int> DeleteEtProducer(EtProducer value)
    {
        return _repository.Delete(value);
    }

    public async Task<List<EtProducer>> GetEtProducers()
    {
        return await _repository.Get();
    }
    
    public List<EtProducer> GetEtProducersByRealId(int? realId)
    {
        return _repository.GetByRealId(realId).ToList();
    }

    public IQueryable<EtProducer> GetQueryableEtProducers()
    {
        return _repository.GetQueryable();
    }
    
    public async Task<EtProducer> GetEtProducerAsync(int id)
    {
        return await _repository.GetAsync(id);
    }

    public async Task<EtProducer> GetEtProducerAsync(EtProducer value)
    {
        return await _repository.GetAsync(value);
    }

    public async Task<EtProducer> GetRealIdAsyncByCode(string marketPrefix)
    {
        return await _repository.GetRealIdAsyncByCode(marketPrefix);
    }
    
    public async Task<EtProducer> GetRealIdAsyncByName(string name)
    {
        return await _repository.GetRealIdAsyncByName(name);
    }
    
    public async Task<int> UpdateEtProducer(EtProducer value)
    {
        return await _repository.Update(value);
    }

    public async Task<int> SaveChanges()
    {
        return await _repository.SaveChanges();
    }
}
