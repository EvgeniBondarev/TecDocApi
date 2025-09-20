using Microsoft.EntityFrameworkCore;
using OzonDomains.Models.MatchedRowSys;
using OzonRepositories.Context;

namespace OzonRepositories.Data
{
    public class MatchedResultRepository : MainRepository, IRepository<MatchedResult>
    {
        public MatchedResultRepository(OzonOrderContext orderContext) : base(orderContext)
        {
        }

        public async Task<int> Add(MatchedResult value)
        {
            _context.MatchedResults.Add(value);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> Delete(MatchedResult value)
        {
            var matchedResult = _context.MatchedResults
                                        .Include(m => m.MatchedRows)
                                        .Include(m => m.MatchedColumns)
                                        .FirstOrDefault(m => m.Id == value.Id);

            if (matchedResult != null)
            {
                if (matchedResult.MatchedRows != null && matchedResult.MatchedRows.Any())
                {
                    _context.MatchedRows.RemoveRange(matchedResult.MatchedRows);
                }
                _context.MatchedResults.Remove(matchedResult);
            }
            return await _context.SaveChangesAsync();
        }

        public async Task<int> Delete(int id)
        {
            var valueToDelete = _context.MatchedResults
                                        .Include(m => m.MatchedRows)
                                        .Include(m => m.MatchedColumns)
                                        .FirstOrDefault(v => v.Id == id);

            if (valueToDelete != null)
            {
                _context.MatchedRows.RemoveRange(valueToDelete.MatchedRows);
                _context.MatchedResults.Remove(valueToDelete);
                return await _context.SaveChangesAsync();
            }

            return 0;
        }


        public async Task<List<MatchedResult>> Get()
        {
            return await _context.MatchedResults
                .Include(m => m.MatchedRows)
                .Include(m => m.MatchedColumns)
                .ToListAsync();
        }

        public async Task<List<MatchedResult>> GetBase()
        {
            return await _context.MatchedResults.ToListAsync();
        }

        public async Task<MatchedResult> GetAsync(int id)
        {
            return await _context.MatchedResults
                .Include(m => m.MatchedRows)
                .Include(m => m.MatchedColumns)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<MatchedResult> GetAsync(MatchedResult value)
        {
            return await _context.MatchedResults
                .Include(m => m.MatchedRows)
                .Include(m => m.MatchedColumns)
                .FirstOrDefaultAsync(m => m.MainFileName == value.MainFileName && m.ScondaryFileName == value.ScondaryFileName);
        }

        public async Task<int> Update(MatchedResult value)
        {
            _context.MatchedResults.Update(value);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> UpdateCascade(MatchedResult value)
        {
            var existingMatchedResult = await _context.MatchedResults
                .Include(m => m.MatchedRows)
                .Include(m => m.MatchedColumns)
                .FirstOrDefaultAsync(m => m.Id == value.Id);

            if (existingMatchedResult == null)
            {
                return -1;
            }

            existingMatchedResult.MatchedColumns = value.MatchedColumns;
            existingMatchedResult.MatchedRows = value.MatchedRows;
            existingMatchedResult.ScondaryFileName = value.ScondaryFileName;
            existingMatchedResult.MainFileName = value.MainFileName;

            var saveChangesResult  = _context.SaveChanges();

            return saveChangesResult;
        }


    }
}
