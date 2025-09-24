using OzonDomains.Models;
using OzonRepositories.Data;

namespace Servcies.DataServcies;

public class WarehouseMappingDataServcies : IDataServcies
{
    private readonly WarehouseMappingRepository _repository;

    public WarehouseMappingDataServcies(WarehouseMappingRepository repository)
    {
        _repository = repository;
    }

    public async Task<int> AddWarehouseMapping(WarehouseMapping value)
    {
        return await _repository.Add(value);
    }

    public async Task<int> DeleteWarehouseMapping(WarehouseMapping value)
    {
        return await _repository.Delete(value);
    }

    public async Task<List<WarehouseMapping>> GetWarehouseMappings()
    {
        return await _repository.Get();
    }

    public async Task<WarehouseMapping> GetWarehouseMappingAsync(int id)
    {
        return await _repository.GetAsync(id);
    }

    public async Task<WarehouseMapping> GetWarehouseMappingAsync(WarehouseMapping value)
    {
        return await _repository.GetAsync(value);
    }
    
    public async Task<WarehouseMapping> GetByOzonName(string ozonName)
    {
        return await _repository.GetByOzonName(ozonName);
    }

    public async Task<int> UpdateWarehouseMapping(WarehouseMapping value)
    {
        return await _repository.Update(value);
    }

    public Task<int> SaveChanges()
    {
        return _repository.SaveChanges();
    }
}