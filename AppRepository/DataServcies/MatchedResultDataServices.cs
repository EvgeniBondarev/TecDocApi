using OzonDomains.Models.MatchedRowSys;
using OzonRepositories.Data;

namespace Servcies.DataServcies
{
    public class MatchedResultDataServices : IDataServcies
    {
        private readonly MatchedResultRepository _repository;

        public MatchedResultDataServices(MatchedResultRepository repository)
        {
            _repository = repository;
        }

        public async Task<int> AddMatchedResult(MatchedResult value)
        {
            return await _repository.Add(value);
        }

        public async Task<int> DeleteMatchedResult(MatchedResult value)
        {
            return await _repository.Delete(value);
        }
        public async Task<int> DeleteMatchedResult(int id)
        {
            return await _repository.Delete(id);
        }

        public async Task<List<MatchedResult>> GetMatchedResults()
        {
            return await _repository.Get();
        }

        public async Task<List<MatchedResult>> GetBaseMatchedResults()
        {
            return await _repository.GetBase();
        }

        public async Task<MatchedResult> GetMatchedResultAsync(int id)
        {
            return await _repository.GetAsync(id);
        }

        public async Task<MatchedResult> GetMatchedResultAsync(MatchedResult value)
        {
            return await _repository.GetAsync(value);
        }

        public async Task<int> UpdateMatchedResult(MatchedResult value)
        {
            return await _repository.UpdateCascade(value);
        }

        public Task<int> SaveChanges()
        {
            return _repository.SaveChanges();
        }
    }
}
