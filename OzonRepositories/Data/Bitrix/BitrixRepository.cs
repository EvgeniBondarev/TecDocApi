using Microsoft.EntityFrameworkCore;
using OzonDomains.Models.BitrixModels;
using OzonRepositories.Context;

namespace OzonRepositories.Data.Bitrix;

public class BitrixRepository : IBitrixRepository
{
    private readonly BitrixContext _context;

    public BitrixRepository(BitrixContext context)
    {
        _context = context;
    }

    public List<BIblockElementProperty> GetBIblockElementPropertiesByArticle(string article)
    {
        return _context.BIblockElementProperties.Where(pr => pr.Value==article && pr.Description == "Артикул").ToList();
    }

    public async Task<BCatalogPrice> GetBCatalogPriceByIblockElementId(int iblockElementId)
    {
        return await _context.BCatalogPrices.FirstOrDefaultAsync(pr => pr.ProductId == iblockElementId);
    }

    public async Task<List<BCatalogStoreProduct>> GetBCatalogStoreProductsByProductId(int productId)
    {
        return await _context.BCatalogStoreProducts.Where(pr => pr.ProductId == productId && pr.Amount != 0).ToListAsync();
    }

    public async Task<BCatalogStore> GetBCatalogStoreById(int id)
    {
        return await _context.BCatalogStores.FindAsync(id);
    }

    public async Task<BCatalogGroup> GetBCatalogGroupById(int id)
    {
        return await _context.BCatalogGroups.FindAsync(id);
    }
}