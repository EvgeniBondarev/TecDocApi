using OzonDomains.Models;
using OzonRepositories.Data;

namespace Servcies.DataServcies
{
    public class ColumnMappingDataServcies : IDataServcies
    {
        private readonly ColumnMappingRepository _repository;

        public ColumnMappingDataServcies(ColumnMappingRepository repository)
        {
            _repository = repository;
        }

        public async Task<int> AddColumnMapping(ColumnMapping value)
        {
            return await _repository.Add(value);
        }

        public async Task<int> DeleteColumnMapping(ColumnMapping value)
        {
            return await _repository.Delete(value);
        }

        public async Task<List<ColumnMapping>> GetColumnMappings()
        {
            return await _repository.Get();
        }
        
        public async Task<ColumnMapping> GetColumnMappingAsync(int id)
        {
            return await _repository.GetAsync(id);
        }

        public async Task<ColumnMapping> GetColumnMappingAsync(ColumnMapping value)
        {
            return await _repository.GetAsync(value);
        }

        public async Task<int> UpdateColumnMapping(ColumnMapping value)
        {
            return await _repository.Update(value);
        }

        public Task<int> SaveChanges()
        {
            return _repository.SaveChanges();
        }
    }
}
