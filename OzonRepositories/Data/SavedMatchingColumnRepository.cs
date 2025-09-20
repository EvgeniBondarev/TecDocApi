using OzonRepositories.Context;
using Microsoft.EntityFrameworkCore;
using OzonDomains.Models.MatchedRowSys;

namespace OzonRepositories.Data
{
    public class SavedMatchingColumnRepository : MainRepository, IRepository<SavedMatchingColumn>
    {
        public SavedMatchingColumnRepository(OzonOrderContext orderContext) : base(orderContext)
        {
        }

        public async Task<int> Add(SavedMatchingColumn value)
        {
            await _context.SavedMatchingColumns.AddAsync(value);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> Delete(SavedMatchingColumn value)
        {
            _context.SavedMatchingColumns.Remove(value);
            return await _context.SaveChangesAsync();
        }

        public async Task<List<SavedMatchingColumn>> Get()
        {
            return await _context.SavedMatchingColumns
                .Include(s => s.MatchingColumns) 
                .ToListAsync();
        }

        public async Task<SavedMatchingColumn> GetAsync(int id)
        {
            return await _context.SavedMatchingColumns
                .Include(s => s.MatchingColumns) 
                .FirstOrDefaultAsync(s => s.Id == id) ?? throw new InvalidOperationException();
        }

        public async Task<SavedMatchingColumn> GetAsync(SavedMatchingColumn value)
        {
            return await _context.SavedMatchingColumns
                .Include(s => s.MatchingColumns)
                .FirstOrDefaultAsync(s => s.Name == value.Name);
        }

        public async Task<int> Update(SavedMatchingColumn value)
        {
            _context.SavedMatchingColumns.Update(value);
            return await _context.SaveChangesAsync();
        }
    }
}
