using OzonDomains.Models;
using OzonRepositories.Data;
using Services.CacheServcies.Cache.OzonOrdersCache;

namespace Servcies.DataServcies
{
    public class ManufacturerDataService : IDataServcies
    {
        private readonly ManufacturerRepository _repository;

        public ManufacturerDataService(ManufacturerRepository repository)
        {
            _repository = repository;
        }

        public async Task<int> AddManufacturer(Manufacturer value)
        {
            return await _repository.Add(value);
        }

        public async Task<int> DeleteManufacturer(Manufacturer value)
        {
            return await _repository.Delete(value);
        }

        public async Task<List<Manufacturer>> GetManufacturers()
        {
            return await _repository.Get();
        }

        public async Task<Manufacturer> GetManufacturerAsync(int id)
        {
            return await _repository.GetAsync(id);
        }

        public async Task<Manufacturer> GetManufacturerAsync(Manufacturer value)
        {
            return await _repository.GetAsync(value);
        }

        public async Task<int> UpdateManufacturer(Manufacturer value)
        {
            
            return await _repository.Update(value);
        }


        public async Task<int> SaveChanges()
        {
            return await _repository.SaveChanges();
        }
    }
}
