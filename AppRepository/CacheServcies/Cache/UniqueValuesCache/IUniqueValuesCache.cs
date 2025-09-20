namespace Servcies.CacheServcies.Cache.UniqueValuesCache
{
    public interface IUniqueValuesCache
    {
        Task<Dictionary<string, int>> GetUniqueArticles();
        Task<Dictionary<string, int>> GetUniqueDeliveryCitys();
        Task<Dictionary<string, int>> GetUniqueShipmentNumbers();
        Task UpdateCache();
    }

}
