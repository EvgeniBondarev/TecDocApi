using Microsoft.EntityFrameworkCore;
using OzonDomains.Models;
using OzonRepositories.Context;

namespace OzonRepositories.Data
{
    public class OzonClientRepository : MainRepository, IRepository<OzonClient>
    {
        public OzonClientRepository(OzonOrderContext orderContext) : base(orderContext)
        {
        }

        public async Task<int> Add(OzonClient value)
        {
            await _context.OzonClients.AddAsync(value);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> Delete(OzonClient value)
        {
            _context.OzonClients.Remove(value);
            return await _context.SaveChangesAsync();
        }

        public async Task<List<OzonClient>> Get()
        {
            return await _context.OzonClients.ToListAsync();
        }

        public async Task<OzonClient> GetAsync(int id)
        {
            return await _context.OzonClients.FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<OzonClient> GetAsync(OzonClient value)
        {
            return _context.OzonClients.FirstOrDefault(p => p.Name == value.Name);
        }

        public async Task<int> Update(OzonClient value)
        {
            var ordetToUpdate = _context.OzonClients.FirstOrDefault(o => o.Id == value.Id);

            ordetToUpdate.Name = value.Name;
            ordetToUpdate.ApiKey = value.ApiKey;
            ordetToUpdate.ClientId = value.ClientId;
            ordetToUpdate.CurrencyCode = value.CurrencyCode;
            ordetToUpdate.INNCode = value.INNCode;
            ordetToUpdate.WarehouseName = value.WarehouseName;

            _context.OzonClients.Update(ordetToUpdate);

            return await _context.SaveChangesAsync();
        }
    }
}
