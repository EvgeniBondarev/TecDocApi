using OzonDomains.Models;

public interface IOrderHistoryDataService
{
    Task<int> AddOrderHistory(OrderHistory value);
    Task<int> DeleteOrderHistory(OrderHistory value);
    Task<List<OrderHistory>> GetOrderHistories();
    Task<OrderHistory> GetOrderHistoryAsync(int id);
    Task<List<OrderHistory>> GetByOrderIdAsync(int orderId);
    Task<List<OrderHistory>> GetRecentChangesAsync(int count = 50);
    Task<List<OrderHistory>> GetChangesAfterDateAsync(DateTime afterDate);
    Task<int> UpdateOrderHistory(OrderHistory value);
    Task<int> SaveChanges();
}