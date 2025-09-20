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
        
        public async Task<int> SaveChanges()
        {
           return await _repository.SaveChanges();
        }
    }
}
