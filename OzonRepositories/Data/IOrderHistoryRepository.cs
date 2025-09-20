using OzonDomains.Models;
using OzonRepositories.Data;

public interface IOrderHistoryRepository : IRepository<OrderHistory>
{
    Task<List<OrderHistory>> GetByOrderIdAsync(int orderId);
    Task<List<OrderHistory>> GetRecentChangesAsync(int count = 50);
    Task<List<OrderHistory>> GetChangesAfterDateAsync(DateTime afterDate);
}