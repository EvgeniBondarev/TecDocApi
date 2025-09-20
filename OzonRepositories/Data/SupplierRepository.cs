using Microsoft.EntityFrameworkCore;
using OzonDomains.Models;
using OzonRepositories.Context;

namespace OzonRepositories.Data
{
    public class SupplierRepository : MainRepository, IRepository<Supplier>
    {
        public SupplierRepository(OzonOrderContext orderContext) : base(orderContext)
        {
        }

        public async Task<int> Add(Supplier value)
        {
            await _context.Suppliers.AddAsync(value);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> Delete(Supplier value)
        {
            _context.Remove(value);
            return await _context.SaveChangesAsync();
        }

        public async Task<List<Supplier>> Get()
        {
            return await _context.Suppliers.ToListAsync();
        }

        public async Task<Supplier> GetAsync(int id)
        {
            return await _context.Suppliers.FindAsync(id);
        }

        public async Task<Supplier> GetAsync(Supplier value)
        {
            return await _context.Suppliers.SingleOrDefaultAsync(a => a.Name == value.Name);
        }
        public async Task<Supplier> GetBySiteAsync(Supplier value)
        {
            return await _context.Suppliers.SingleOrDefaultAsync(a => a.Site == value.Site);
        }

        public async Task<List<Supplier>> GetWithUrls()
        {
            return await _context.Suppliers.Where(s => s.Site != null).ToListAsync();
        }

        public async Task<int> Update(Supplier value)
        {
            _context.Suppliers.Update(value);
            return await _context.SaveChangesAsync();
        }
    }
}
