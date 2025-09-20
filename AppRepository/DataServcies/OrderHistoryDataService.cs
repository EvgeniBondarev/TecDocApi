using OzonDomains.Models;

namespace Servcies.DataServcies;

public class OrderHistoryDataService : IOrderHistoryDataService
{
    private readonly IOrderHistoryRepository _repository;

    public OrderHistoryDataService(IOrderHistoryRepository repository)
    {
        _repository = repository;
    }

    public async Task<int> AddOrderHistory(OrderHistory value)
    {
        return await _repository.Add(value);
    }

    public async Task<int> DeleteOrderHistory(OrderHistory value)
    {
        return await _repository.Delete(value);
    }

    public async Task<List<OrderHistory>> GetOrderHistories()
    {
        return await _repository.Get();
    }

    public async Task<OrderHistory> GetOrderHistoryAsync(int id)
    {
        return await _repository.GetAsync(id);
    }

    public async Task<List<OrderHistory>> GetByOrderIdAsync(int orderId)
    {
        return await _repository.GetByOrderIdAsync(orderId);
    }

    public async Task<List<OrderHistory>> GetRecentChangesAsync(int count = 50)
    {
        return await _repository.GetRecentChangesAsync(count);
    }

    public async Task<List<OrderHistory>> GetChangesAfterDateAsync(DateTime afterDate)
    {
        return await _repository.GetChangesAfterDateAsync(afterDate);
    }

    public async Task<int> UpdateOrderHistory(OrderHistory value)
    {
        return await _repository.Update(value);
    }

    public Task<int> SaveChanges()
    {
        throw new NotImplementedException();
    }
}