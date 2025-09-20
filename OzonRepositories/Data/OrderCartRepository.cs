using Microsoft.EntityFrameworkCore;
using OzonDomains.Models;
using OzonDomains.Models.OrderCarts;
using OzonRepositories.Context;

namespace OzonRepositories.Data
{
    public class OrderCartRepository : MainRepository, IRepository<OrderCart>
    {
        private readonly OrderRepository _orderRepository;
        public OrderCartRepository(OzonOrderContext orderContext, OrderRepository orderRepository) : base(orderContext)
        {
            _orderRepository = orderRepository;
        }

        public async Task<int> Add(OrderCart cart)
        {
            var existingCartStatus = await _context.CartStatuses
                .FirstOrDefaultAsync(cs => cs.Name == cart.CartStatus.Name);

            if (existingCartStatus == null)
            {
                _context.CartStatuses.Add(cart.CartStatus);
            }
            else
            {
                cart.CartStatus = existingCartStatus;
            }

            foreach (var item in cart.OrderItems)
            {
                var existingItemStatus = await _context.ItemStatuses
                    .FirstOrDefaultAsync(ist => ist.Name == item.ItemStatus.Name);

                if (existingItemStatus == null)
                {
                    _context.ItemStatuses.Add(item.ItemStatus);
                }
                else
                {
                    item.ItemStatus = existingItemStatus;
                }

                item.OrderCart = cart;
            }

            _context.OrderCarts.Add(cart);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> Delete(OrderCart cart)
        {
            _context.OrderCarts.Remove(cart);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> DeleteAsync(int id)
        {
            var cart = await _context.OrderCarts
                .Include(c => c.OrderItems)
                    .ThenInclude(oi => oi.StudioOrder)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cart == null)
                return 0;

            _context.OrderCarts.Remove(cart);
            return await _context.SaveChangesAsync();
        }

        public async Task<List<OrderCart>> Get()
        {
            var carts = await _context.OrderCarts
                .Include(c => c.CartStatus)
                .Include(c => c.OrderItems)
                .ThenInclude(oi => oi.ItemStatus)
                .ToListAsync();

            // Удаляем carts без OrderItems
            var emptyCarts = carts.Where(c => c.OrderItems == null || !c.OrderItems.Any()).ToList();
            if (emptyCarts.Any())
            {
                _context.OrderCarts.RemoveRange(emptyCarts);
                await _context.SaveChangesAsync();

                // Удаляем их из списка перед возвратом
                carts = carts.Except(emptyCarts).ToList();
            }

            foreach (var cart in carts)
            {
                foreach (var item in cart.OrderItems)
                {
                    if (item.StudioOrderId.HasValue)
                        item.StudioOrder = await _orderRepository.GetAsync(item.StudioOrderId.Value);
                }
            }

            return carts;
        }

        public async Task<OrderItem?> GetItemId(int itemId)
        {
            return await _context.OrderItems
                .FirstOrDefaultAsync(c => c.Id == itemId);
        }

        public async Task<List<OrderCart>> GetWithPagination(int skip, int take)
        {
            var carts = await _context.OrderCarts
                .Include(c => c.CartStatus)
                .Include(c => c.OrderItems)
                .ThenInclude(oi => oi.ItemStatus)
                .OrderByDescending(c => c.CreatedAt) 
                .Skip(skip)
                .Take(take)
                .ToListAsync();

            var emptyCarts = carts.Where(c => c.OrderItems == null || !c.OrderItems.Any()).ToList();
            if (emptyCarts.Any())
            {
                _context.OrderCarts.RemoveRange(emptyCarts);
                await _context.SaveChangesAsync();
                carts = carts.Except(emptyCarts).ToList();
            }

            foreach (var cart in carts)
            {
                foreach (var item in cart.OrderItems)
                {
                    if (item.StudioOrderId.HasValue)
                        item.StudioOrder = await _orderRepository.GetAsyncForCart(item.StudioOrderId.Value);
                }
            }

            return carts;
        }

        public async Task<OrderCart> GetAsync(int id)
        {
            return await _context.OrderCarts
                .Include(c => c.CartStatus)
                .Include(c => c.OrderItems)
                    .ThenInclude(oi => oi.ItemStatus)
                .Include(c => c.OrderItems)
                    .ThenInclude(oi => oi.StudioOrder)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<OrderCart> GetAsync(OrderCart value)
        {
            return await _context.OrderCarts
                .Include(c => c.CartStatus)
                .Include(c => c.OrderItems)
                    .ThenInclude(oi => oi.ItemStatus)
                .Include(c => c.OrderItems)
                    .ThenInclude(oi => oi.StudioOrder)
                .FirstOrDefaultAsync(c => c.Id == value.Id);
        }

        public async Task<int> Update(OrderCart cart)
        {
            _context.OrderCarts.Update(cart);
            return await _context.SaveChangesAsync();
        }
        
        public async Task<int> GetTotalCount()
        {
            return await _context.OrderCarts
                .CountAsync(c => c.OrderItems.Any());
        }
    }
}
