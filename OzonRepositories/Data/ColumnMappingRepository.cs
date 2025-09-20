using Microsoft.EntityFrameworkCore;
using OzonDomains.Models;
using OzonRepositories.Context;

namespace OzonRepositories.Data
{
    public class ColumnMappingRepository : MainRepository, IRepository<ColumnMapping>
    {
        public ColumnMappingRepository(OzonOrderContext orderContext) : base(orderContext)
        {
        }

        public async Task<int> Add(ColumnMapping value)
        {
            _context.ColumnMappings.Add(value);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> Delete(ColumnMapping value)
        {
            _context.ColumnMappings.Remove(value);
            return await _context.SaveChangesAsync();
        }

        public Task<List<ColumnMapping>> Get()
        {
            return _context.ColumnMappings.ToListAsync();
        }
        

        
        public async Task<ColumnMapping> GetAsync(int id)
        {
            return await _context.ColumnMappings.FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<ColumnMapping> GetAsync(ColumnMapping value)
        {
            return await _context.ColumnMappings.FirstOrDefaultAsync(c => c.MappingName == value.MappingName);
        }

        public async Task<int> Update(ColumnMapping value)
        {
            _context.ColumnMappings.Update(value);
            return await _context.SaveChangesAsync();
        }
    }

}
