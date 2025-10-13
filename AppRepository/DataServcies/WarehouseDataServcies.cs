using OzonDomains.Models;
using OzonRepositories.Data;

namespace Servcies.DataServcies
{
    public class WarehouseDataServcies : IDataServcies
    {
        private readonly WarehouseRepository _repository;

        public WarehouseDataServcies(WarehouseRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<Warehouse>> GetWarehouses()
        {
            return await _repository.Get();
        }

        public async Task<Warehouse> GetWarehouseAsync(int id)
        {
            return await _repository.GetAsync(id);
        }

        public async Task<Warehouse> GetWarehouseAsync(Warehouse value)
        {
            return await _repository.GetAsync(value);
        }

        public async Task<Warehouse> GetOrCreateAsync(Warehouse value)
        {
            return await _repository.GetOrCreateAsync(value);
        }


        public async Task<int> SaveChanges()
        {
            return await _repository.SaveChanges();
        }
    }
}