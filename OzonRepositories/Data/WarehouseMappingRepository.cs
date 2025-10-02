using Microsoft.EntityFrameworkCore;
using OzonDomains.Models;
using OzonRepositories.Context;

namespace OzonRepositories.Data;

public class WarehouseMappingRepository : MainRepository, IRepository<WarehouseMapping>
{
    public WarehouseMappingRepository(OzonOrderContext orderContext) : base(orderContext)
    {
    }

    public async Task<int> Add(WarehouseMapping value)
    {
        _context.WarehouseMappings.Add(value);
        return await _context.SaveChangesAsync();
    }

    public async Task<int> Delete(WarehouseMapping value)
    {
        _context.WarehouseMappings.Remove(value);
        return await _context.SaveChangesAsync();
    }

    public Task<List<WarehouseMapping>> Get()
    {
        return _context.WarehouseMappings
            .Include(wm => wm.OzonClient) 
            .ToListAsync();
    }

    public async Task<WarehouseMapping> GetAsync(int id)
    {
        return await _context.WarehouseMappings
            .Include(wm => wm.OzonClient) 
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<WarehouseMapping> GetAsync(WarehouseMapping value)
    {
        return await _context.WarehouseMappings
            .Include(wm => wm.OzonClient) 
            .FirstOrDefaultAsync(c =>
                c.BitrixWarehouseName == value.BitrixWarehouseName &&
                c.OzonWarehouseName == value.OzonWarehouseName &&
                c.OzonClientId == value.OzonClientId); 
    }
    
    public async Task<WarehouseMapping> GetByOzonName(string ozonName)
    {
        return await _context.WarehouseMappings.FirstOrDefaultAsync(w => w.OzonWarehouseName == ozonName);
    }

    // Новый метод для поиска по ID клиента
    public async Task<List<WarehouseMapping>> GetByClientIdAsync(int clientId)
    {
        return await _context.WarehouseMappings
            .Include(wm => wm.OzonClient)
            .Where(wm => wm.OzonClientId == clientId)
            .ToListAsync();
    }

    // Новый метод для поиска конкретного сопоставления по всем параметрам
    public async Task<WarehouseMapping> GetExactAsync(string bitrixName, string ozonName, int clientId)
    {
        return await _context.WarehouseMappings
            .Include(wm => wm.OzonClient)
            .FirstOrDefaultAsync(c =>
                c.BitrixWarehouseName == bitrixName &&
                c.OzonWarehouseName == ozonName &&
                c.OzonClientId == clientId);
    }

    public async Task<int> Update(WarehouseMapping value)
    {
        _context.WarehouseMappings.Update(value);
        return await _context.SaveChangesAsync();
    }
}