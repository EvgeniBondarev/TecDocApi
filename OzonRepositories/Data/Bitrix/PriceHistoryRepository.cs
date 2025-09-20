using Microsoft.EntityFrameworkCore;
using OzonRepositories.Context;

namespace OzonRepositories.Data.Bitrix;

public class PriceHistoryRepository : MainRepository, IRepository<PriceHistory>
{
    public PriceHistoryRepository(OzonOrderContext orderContext) : base(orderContext)
    {
    }

    public async Task<int> Add(PriceHistory value)
    {
        _context.PriceHistories.Add(value);
        return await _context.SaveChangesAsync();
    }

    public async Task<int> Delete(PriceHistory value)
    {
        _context.PriceHistories.Remove(value);
        return await _context.SaveChangesAsync();
    }

    public Task<List<PriceHistory>> Get()
    {
        return _context.PriceHistories.ToListAsync();
    }

    public async Task<PriceHistory> GetAsync(int id)
    {
        return await _context.PriceHistories.FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<PriceHistory> GetAsync(PriceHistory value)
    {
        return await _context.PriceHistories.FirstOrDefaultAsync(c => c.Article == value.Article);
    }

    public async Task<List<PriceHistory>> GetByArticleAsync(string article)
    {
        return await _context.PriceHistories
            .Where(ph => ph.Article == article)
            .OrderByDescending(ph => ph.CreateDateTime)
            .ToListAsync();
    }

    public async Task<List<PriceHistory>> GetByBitrixIdAsync(int bitrixId)
    {
        return await _context.PriceHistories
            .Where(ph => ph.BitrixId == bitrixId)
            .OrderByDescending(ph => ph.CreateDateTime)
            .ToListAsync();
    }
    
    public async Task<PriceHistory?> GetLastByBitrixIdAsync(int bitrixId)
    {
        return await _context.PriceHistories
            .Where(ph => ph.BitrixId == bitrixId)
            .OrderByDescending(ph => ph.CreateDateTime)
            .FirstOrDefaultAsync();
    }
    
    public async Task<Dictionary<int, PriceHistory>> GetLastByBitrixIdsAsync(List<int> bitrixIds)
    {
        return await _context.PriceHistories
            .Where(ph => bitrixIds.Contains(ph.BitrixId))
            .GroupBy(ph => ph.BitrixId)
            .Select(g => g.OrderByDescending(ph => ph.CreateDateTime).FirstOrDefault())
            .ToDictionaryAsync(ph => ph.BitrixId, ph => ph);
    }
    
    public async Task<List<PriceHistory>> GetLastRecordsAsync(int count)
    {
        return await _context.PriceHistories
            .OrderByDescending(ph => ph.CreateDateTime)
            .Take(count)
            .ToListAsync();
    }

    public async Task<int> Update(PriceHistory value)
    {
        _context.PriceHistories.Update(value);
        return await _context.SaveChangesAsync();
    }
}