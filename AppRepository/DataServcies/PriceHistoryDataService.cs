using OzonRepositories.Data.Bitrix;

namespace Servcies.DataServcies;

public class PriceHistoryDataService : IPriceHistoryDataService
{
    private readonly PriceHistoryRepository _repository;

    public PriceHistoryDataService(PriceHistoryRepository repository)
    {
        _repository = repository;
    }

    public async Task<int> AddPriceHistory(PriceHistory value)
    {
        return await _repository.Add(value);
    }

    public async Task<int> DeletePriceHistory(PriceHistory value)
    {
        return await _repository.Delete(value);
    }

    public async Task<List<PriceHistory>> GetPriceHistories()
    {
        return await _repository.Get();
    }

    public async Task<PriceHistory> GetPriceHistoryAsync(int id)
    {
        return await _repository.GetAsync(id);
    }

    public async Task<PriceHistory> GetPriceHistoryAsync(PriceHistory value)
    {
        return await _repository.GetAsync(value);
    }

    public async Task<List<PriceHistory>> GetPriceHistoriesByArticleAsync(string article)
    {
        return await _repository.GetByArticleAsync(article);
    }
    
    public async Task<List<PriceHistory>> GetLastPriceHistoriesAsync(int count)
    {
        return await _repository.GetLastRecordsAsync(count);
    }

    public async Task<Dictionary<int, PriceHistory>> GetLastByBitrixIdsAsync(List<int> bitrixIds)
    {
        return await _repository.GetLastByBitrixIdsAsync(bitrixIds);
    }

    public async Task<List<PriceHistory>> GetPriceHistoriesByBitrixIdAsync(int bitrixId)
    {
        return await _repository.GetByBitrixIdAsync(bitrixId);
    }

    public async Task<PriceHistory?> GetLastByBitrixIdAsync(int bitrixId)
    {
        return await _repository.GetLastByBitrixIdAsync(bitrixId);
    }
    
    

    public async Task<int> UpdatePriceHistory(PriceHistory value)
    {
        return await _repository.Update(value);
    }

    public Task<int> SaveChanges()
    {
        return _repository.SaveChanges();
    }
}