using Microsoft.EntityFrameworkCore;
using OzonDomains.Models;
using OzonRepositories.Context;

namespace OzonRepositories.Data
{
    public class TransactionRepository : MainRepository
    {
        public TransactionRepository(OzonOrderContext orderContext) : base(orderContext)
        {
        }

        public async Task<int> Add(Transaction value)
        {
           await _context.Transactions.AddAsync(value);  
            return await _context.SaveChangesAsync();
        }

        public async Task<int> Delete(Transaction value)
        {
            _context.Remove(value);
            return await _context.SaveChangesAsync();
        }

        public IQueryable<Transaction> Get()
        {
            return _context.Transactions
                           .Include(tr => tr.Orders)
                               .ThenInclude(o => o.Сurrency)
                           .Include(tr => tr.Orders)
                               .ThenInclude(o => o.ShipmentWarehouse)
                           .Include(tr => tr.Orders)
                               .ThenInclude(o => o.ProductInfo)
                           .Include(tr => tr.Orders)
                               .ThenInclude(o => o.AppStatus)
                           .Include(tr => tr.Orders)
                               .ThenInclude(o => o.Supplier)
                           .Include(tr => tr.Orders)
                               .ThenInclude(o => o.OzonClient)
                           .Include(tr => tr.Orders)
                               .ThenInclude(o => o.Manufacturer);
        }

        public async Task<Transaction> GetAsync(int id)
        {
            return (await _context.Transactions
                .Include(tr => tr.Orders)
                .ThenInclude(o => o.Сurrency)
                .Include(tr => tr.Orders)
                .ThenInclude(o => o.ShipmentWarehouse)
                .Include(tr => tr.Orders)
                .ThenInclude(o => o.ProductInfo)
                .Include(tr => tr.Orders)
                .ThenInclude(o => o.AppStatus)
                .Include(tr => tr.Orders)
                .ThenInclude(o => o.Supplier)
                .Include(tr => tr.Orders)
                .ThenInclude(o => o.OzonClient)
                .Include(tr => tr.Orders)
                .ThenInclude(o => o.Manufacturer)
                .FirstOrDefaultAsync(t => t.Id == id))!;
        }

        public async Task<Transaction> GetAsync(Transaction value)
        {
            return (await _context.Transactions
                .Include(tr => tr.Orders)
                .ThenInclude(o => o.Сurrency)
                .Include(tr => tr.Orders)
                .ThenInclude(o => o.ShipmentWarehouse)
                .Include(tr => tr.Orders)
                .ThenInclude(o => o.ProductInfo)
                .Include(tr => tr.Orders)
                .ThenInclude(o => o.AppStatus)
                .Include(tr => tr.Orders)
                .ThenInclude(o => o.Supplier)
                .Include(tr => tr.Orders)
                .ThenInclude(o => o.OzonClient)
                .Include(tr => tr.Orders)
                .ThenInclude(o => o.Manufacturer)
                .FirstOrDefaultAsync(t => t.Id == value.Id))!; ;
        }

        public async Task<int> Update(Transaction value)
        {
            _context.Update(value);
            return await _context.SaveChangesAsync();
        }
        
        public IQueryable<Transaction> GetTransactionsWithPagination(int skip, int take)
        {
            return _context.Transactions
                           .Include(tr => tr.Orders)
                               .ThenInclude(o => o.Сurrency)
                           .Include(tr => tr.Orders)
                               .ThenInclude(o => o.ShipmentWarehouse)
                           .Include(tr => tr.Orders)
                               .ThenInclude(o => o.ProductInfo)
                           .Include(tr => tr.Orders)
                               .ThenInclude(o => o.AppStatus)
                           .Include(tr => tr.Orders)
                               .ThenInclude(o => o.Supplier)
                           .Include(tr => tr.Orders)
                               .ThenInclude(o => o.OzonClient)
                           .Include(tr => tr.Orders)
                               .ThenInclude(o => o.Manufacturer)
                           .OrderByDescending(tr => tr.CreatedDateTime)
                           .Skip(skip)
                           .Take(take);
        }
        
        public async Task<int> GetTotalTransactionCount()
        {
            return await _context.Transactions.CountAsync();
        }
    }
}
