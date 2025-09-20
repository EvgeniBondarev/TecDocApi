using OzonDomains.Models.MatchedRowSys;
using OzonRepositories.Data;

namespace Servcies.DataServcies
{
    public class MatchedRowDataServices : IDataServcies
    {
        private readonly MatchedRowRepository _repository;

        public MatchedRowDataServices(MatchedRowRepository repository)
        {
            _repository = repository;
        }

        public async Task<int> AddMatchedRow(MatchedRow value)
        {
            return await _repository.Add(value);
        }

        public async Task<int> AddMatchedRows(List<MatchedRow> values)
        {
            return await _repository.AddRange(values);
        }

        public async Task<int> DeleteMatchedRow(MatchedRow value)
        {
            return await _repository.Delete(value);
        }

        public async Task<List<MatchedRow>> GetMatchedRows()
        {
            return await _repository.Get();
        }

        public async Task<MatchedRow> GetMatchedRowAsync(int id)
        {
            return await _repository.GetAsync(id);
        }

        public async Task<MatchedRow> GetMatchedRowAsync(MatchedRow value)
        {
            return await _repository.GetAsync(value);
        }

        public async Task<int> UpdateMatchedRow(MatchedRow value)
        {
            return await _repository.Update(value);
        }

        public Task<int> SaveChanges()
        {
            return _repository.SaveChanges();
        }
    }
}
