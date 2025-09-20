using OzonDomains.Models;
using OzonDomains.Models.MatchedRowSys;
using OzonRepositories.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Servcies.DataServcies
{
    public class SavedMatchingColumnDataServcies : IDataServcies
    {
        private readonly SavedMatchingColumnRepository _repository;

        public SavedMatchingColumnDataServcies(SavedMatchingColumnRepository repository)
        {
            _repository = repository;
        }

        public Task<int> AddSavedMatchingColumn(SavedMatchingColumn value)
        {
            return _repository.Add(value);
        }
        public async Task<List<SavedMatchingColumn>> GetSavedMatchingColumns()
        {
            return await _repository.Get();
        }

        public async Task<SavedMatchingColumn> GetSavedMatchingColumnAsync(int id)
        {
            return await _repository.GetAsync(id);
        }
        public async Task<int> SaveChanges()
        {
            return await _repository.SaveChanges();
        }
    }
}
