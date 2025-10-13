using Microsoft.EntityFrameworkCore;
using OzonDomains.Models;
using OzonRepositories.Context;

namespace OzonRepositories.Data
{
    public class WarehouseRepository : MainRepository, IRepository<Warehouse>
    {
        public WarehouseRepository(OzonOrderContext orderContext) : base(orderContext)
        {
        }

        public async Task<int> Add(Warehouse value)
        {
            await _context.Warehouses.AddAsync(value);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> Delete(Warehouse value)
        {
            _context.Warehouses.Remove(value);
            return await _context.SaveChangesAsync();
        }

        public async Task<List<Warehouse>> Get()
        {
            return await _context.Warehouses.ToListAsync();
        }

        public async Task<Warehouse> GetAsync(int id)
        {
            return await _context.Warehouses.FirstOrDefaultAsync(w => w.Id == id);
        }

        public async Task<Warehouse> GetAsync(Warehouse value)
        {
            return await _context.Warehouses.FirstOrDefaultAsync(w => w.Name == value.Name);
        }

        public async Task<Warehouse> GetOrCreateAsync(Warehouse value)
        {
            var existing = await _context.Warehouses
                .FirstOrDefaultAsync(w => w.Name == value.Name);

            if (existing != null)
                return existing;

            // Если не найден — создаём новый
            _context.Warehouses.Add(value);
            await _context.SaveChangesAsync();

            return value;
        }

        public async Task<int> Update(Warehouse value)
        {
            _context.Warehouses.Update(value);
            return await _context.SaveChangesAsync();
        }
    }
}