using Microsoft.EntityFrameworkCore;
using OzonDomains.Models;
using OzonRepositories.Context;

namespace OzonRepositories.Data;

public class OrderHistoryRepository : MainRepository, IOrderHistoryRepository
    {
        public OrderHistoryRepository(OzonOrderContext orderContext) : base(orderContext)
        {
        }

        public async Task<int> Add(OrderHistory value)
        {
            _context.OrderHistories.Add(value);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> Delete(OrderHistory value)
        {
            _context.OrderHistories.Remove(value);
            return await _context.SaveChangesAsync();
        }

        public Task<List<OrderHistory>> Get()
        {
            return _context.OrderHistories
                .Include(oh => oh.Order)
                .OrderByDescending(oh => oh.ChangedAt)
                .ToListAsync();
        }

        public async Task<OrderHistory> GetAsync(int id)
        {
            return await _context.OrderHistories
                .Include(oh => oh.Order)
                .FirstOrDefaultAsync(oh => oh.Id == id);
        }

        public async Task<OrderHistory> GetAsync(OrderHistory value)
        {
            return await _context.OrderHistories
                .FirstOrDefaultAsync(oh => oh.OrderId == value.OrderId && 
                                         oh.ColumnName == value.ColumnName &&
                                         oh.ChangedAt == value.ChangedAt);
        }

        public async Task<int> Update(OrderHistory value)
        {
            _context.OrderHistories.Update(value);
            return await _context.SaveChangesAsync();
        }

        public async Task<List<OrderHistory>> GetByOrderIdAsync(int orderId)
        {
            return await _context.OrderHistories
                .Where(oh => oh.OrderId == orderId)
                .Include(oh => oh.Order)
                    .ThenInclude(c => c.OzonClient)
                .OrderByDescending(oh => oh.ChangedAt)
                .ToListAsync();
        }

        public async Task<List<OrderHistory>> GetRecentChangesAsync(int count = 50)
        {
            return await _context.OrderHistories
                .OrderByDescending(oh => oh.ChangedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<OrderHistory>> GetChangesAfterDateAsync(DateTime afterDate)
        {
            return await _context.OrderHistories
                .Where(oh => oh.ChangedAt > afterDate)
                .OrderByDescending(oh => oh.ChangedAt)
                .ToListAsync();
        }
        
        
    }