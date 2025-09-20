using Microsoft.EntityFrameworkCore;
using OzonDomains.Models;
using OzonRepositories.Context;

namespace OzonRepositories.Data;

public class EtProducerRepository : IRepository<EtProducer>
{
    JcEtalonContext _context;

    public EtProducerRepository(JcEtalonContext context)
    {
        _context = context;
    }
    
    public async Task<int> Add(EtProducer value)
    {
        await _context.EtProducers.AddAsync(value);
        return await _context.SaveChangesAsync();
    }

    public async Task<int> Delete(EtProducer value)
    {
        _context.EtProducers.Remove(value);
        return await _context.SaveChangesAsync();
    }
    
    public async Task<List<EtProducer>> Get()
    {
        return await _context.EtProducers.ToListAsync();
    }
    
    public IQueryable<EtProducer> GetByRealId(int? realId)
    {
        return _context.EtProducers.Where(e => e.RealId == realId);
    }
    
    public IQueryable<EtProducer> GetQueryable()
    {
        return _context.EtProducers.AsQueryable();
    }
    
    public async Task<EtProducer> GetAsync(int id)
    {
        return await _context.EtProducers.FindAsync(id);
    }
    
    public async Task<EtProducer> GetAsync(EtProducer value)
    {
        return await _context.EtProducers.SingleOrDefaultAsync(e => e.MarketPrefix == value.MarketPrefix);
    }

    public async Task<EtProducer> GetRealIdAsyncByCode(string marketPrefix)
    {
       return await _context.EtProducers
            .Where(r => r.MarketPrefix == marketPrefix || r.Prefix == marketPrefix)
            .FirstOrDefaultAsync();
    }


    public async Task<EtProducer> GetRealIdAsyncByName(string name)
    {
        if (string.IsNullOrEmpty(name))
            return null;
        
        // Получаем все и фильтруем в памяти
        var allProducers = await _context.EtProducers
            .Where(p => p.Name != null)
            .AsNoTracking()
            .ToListAsync();
        
        return allProducers.FirstOrDefault(p => 
            p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }
    
    public async Task<int> Update(EtProducer value)
    {
        var producerToUpdate = await _context.EtProducers.FindAsync(value.Id);
        if (producerToUpdate != null)
        {
            producerToUpdate.MarketPrefix = value.MarketPrefix;
            producerToUpdate.Name = value.Name;
        }
        return await _context.SaveChangesAsync();
    }
    
    public async Task<Dictionary<string, string?>> GetMarketPrefixesByNamesAsync(List<string> names)
    {
        if (names == null || names.Count == 0)
            return new Dictionary<string, string?>();

        var allProducers = await _context.EtProducers
            .Where(p => p.Prefix != null)
            .Select(p => new { p.Id, p.RealId, p.Prefix, p.MarketPrefix })
            .AsNoTracking()
            .ToListAsync();

        var producersById = allProducers.ToDictionary(p => p.Id);

        var normalizedNames = new HashSet<string>(
            names.Where(name => !string.IsNullOrEmpty(name))
                 .Select(name => name.Replace(" ", "").ToLower())
        );

        var producersByNormalizedPrefix = allProducers
            .Where(p => !string.IsNullOrEmpty(p.Prefix))
            .ToLookup(p => p.Prefix!.Replace(" ", "").ToLower());

        var result = new Dictionary<string, string?>();

        foreach (var originalName in names)
        {
            if (string.IsNullOrEmpty(originalName))
            {
                result[originalName] = null;
                continue;
            }

            var normalized = originalName.Replace(" ", "").ToLower();
            var matches = producersByNormalizedPrefix[normalized].ToList();

            string? marketPrefix = null;

            if (matches.Count > 0)
            {
                var match = matches[0]; 

                if (match.Id == match.RealId)
                {
                    marketPrefix = match.MarketPrefix;
                }
                else if (match.RealId != 0 && producersById.TryGetValue(match.RealId, out var realProducer))
                {
                    marketPrefix = realProducer.MarketPrefix;
                }
                else
                {
                    marketPrefix = match.MarketPrefix;
                }
            }

            result[originalName] = marketPrefix;
        }
        return result;
    }

    
    public async Task<int> SaveChanges()
    {
        return await _context.SaveChangesAsync();
    }
}