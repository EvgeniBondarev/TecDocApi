using Microsoft.EntityFrameworkCore;
using OzonDomains.Models;
using OzonRepositories.Context;

namespace OzonRepositories.Data
{
    public class OrdersFileMetadataRepository : MainRepository, IRepository<OrdersFileMetadata>
    {
        public OrdersFileMetadataRepository(OzonOrderContext orderContext) : base(orderContext)
        {
        }

        public async Task<int> Add(OrdersFileMetadata value)
        {
            await _context.OrdersFileMetadata.AddAsync(value);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> Delete(OrdersFileMetadata value)
        {
            _context.OrdersFileMetadata.Remove(value);
            return await _context.SaveChangesAsync();
        }

        public async Task<List<OrdersFileMetadata>> Get()
        {
            return await _context.OrdersFileMetadata.ToListAsync();
        }

        public async Task<OrdersFileMetadata> GetAsync(int id)
        {
            return await _context.OrdersFileMetadata.FindAsync(id);
        }

        public async Task<OrdersFileMetadata> GetAsync(OrdersFileMetadata value)
        {
            return await _context.OrdersFileMetadata
                .SingleOrDefaultAsync(a => a.FolderName == value.FolderName && a.FileName == value.FileName);
        }

        public async Task<int> Update(OrdersFileMetadata value)
        {
            _context.OrdersFileMetadata.Update(value);
            return await _context.SaveChangesAsync();
        }
    }

}
