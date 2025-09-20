using OzonRepositories.Data.Bitrix;

namespace Servcies.DataServcies;

public interface IPriceHistoryDataService
{
    Task<int> AddPriceHistory(PriceHistory value);
    Task<int> DeletePriceHistory(PriceHistory value);
    Task<List<PriceHistory>> GetPriceHistories();
    Task<PriceHistory> GetPriceHistoryAsync(int id);
    Task<PriceHistory> GetPriceHistoryAsync(PriceHistory value);
    Task<List<PriceHistory>> GetPriceHistoriesByArticleAsync(string article);
    Task<List<PriceHistory>> GetLastPriceHistoriesAsync(int count);
    Task<Dictionary<int, PriceHistory>> GetLastByBitrixIdsAsync(List<int> bitrixIds);
    Task<List<PriceHistory>> GetPriceHistoriesByBitrixIdAsync(int bitrixId);
    Task<PriceHistory?> GetLastByBitrixIdAsync(int bitrixId);
    Task<int> UpdatePriceHistory(PriceHistory value);
    Task<int> SaveChanges();
}