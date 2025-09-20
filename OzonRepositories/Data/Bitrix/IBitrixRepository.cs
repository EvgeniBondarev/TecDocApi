using OzonDomains.Models.BitrixModels;

namespace OzonRepositories.Data.Bitrix;

public interface IBitrixRepository
{
   List<BIblockElementProperty> GetBIblockElementPropertiesByArticle(string article);
    Task<BCatalogPrice> GetBCatalogPriceByIblockElementId(int iblockElementId);
    Task<List<BCatalogStoreProduct>> GetBCatalogStoreProductsByProductId(int productId);
    Task<BCatalogStore> GetBCatalogStoreById(int id);
    Task<BCatalogGroup> GetBCatalogGroupById(int id);
}