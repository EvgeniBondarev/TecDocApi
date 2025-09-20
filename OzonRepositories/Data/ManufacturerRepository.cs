using Microsoft.EntityFrameworkCore;
using OzonDomains.Models;
using OzonRepositories.Context;

namespace OzonRepositories.Data
{
    public class ManufacturerRepository : MainRepository, IRepository<Manufacturer>
    {
        public ManufacturerRepository(OzonOrderContext orderContext) : base(orderContext)
        {
        }

        public async Task<int> Add(Manufacturer value)
        {
            _context.Manufacturers.Add(value);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> Delete(Manufacturer value)
        {
            _context.Manufacturers.Remove(value);
            return await (_context.SaveChangesAsync());
        }

        public async Task<List<Manufacturer>> Get()
        {
            return await _context.Manufacturers.ToListAsync();
        }

        public async Task<Manufacturer> GetAsync(int id)
        {
            return _context.Manufacturers.FirstOrDefault(x => x.Id == id);
        }

        public async Task<Manufacturer> GetAsync(Manufacturer value)
        {
            return _context.Manufacturers.FirstOrDefault(x => x.Code == value.Code);
        }

        public async Task<int> Update(Manufacturer value)
        {
            Manufacturer manufacturer = await _context.Manufacturers.FindAsync(value.Id);
            if (manufacturer != null)
            {
                manufacturer.Name = value.Name;
                manufacturer.Code = value.Code;
            }

            return await _context.SaveChangesAsync();
        }
    }
}
