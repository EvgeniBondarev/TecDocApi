using Microsoft.EntityFrameworkCore;
using OzonDomains;
using OzonDomains.Models.OrderCarts;
using OzonRepositories.Context;

namespace OzonRepositories.Data;

 public class OrderItemRepository : MainRepository, IRepository<OrderItem>
    {
        private readonly OrderRepository _orderRepository;

        public OrderItemRepository(OzonOrderContext context, OrderRepository orderRepository) : base(context)
        {
            _orderRepository = orderRepository;
        }

        public async Task<int> Add(OrderItem item)
        {
            var existingItemStatus = await _context.ItemStatuses
                .FirstOrDefaultAsync(s => s.Name == item.ItemStatus.Name);

            if (item.ItemStatus != null)
            {
                if (existingItemStatus == null)
                {
                    _context.ItemStatuses.Add(item.ItemStatus);
                }
                else
                {
                    item.ItemStatus = existingItemStatus;
                }
            }

            _context.OrderItems.Add(item);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> Delete(OrderItem item)
        {
            _context.OrderItems.Remove(item);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> DeleteAsync(int id)
        {
            var item = await _context.OrderItems
                .Include(i => i.StudioOrder)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (item == null)
                return 0;

            _context.OrderItems.Remove(item);
            return await _context.SaveChangesAsync();
        }

        public async Task<List<OrderItem>> Get()
        {
            var items = await _context.OrderItems
                .Include(i => i.ItemStatus)
                .ToListAsync();

            foreach (var item in items)
            {
                if (item.StudioOrderId.HasValue)
                {
                    item.StudioOrder = await _orderRepository.GetAsync(item.StudioOrderId.Value);
                }
            }

            return items;
        }
        public async Task<List<OrderItem>> GetLast(int count)
        {
            var items = await _context.OrderItems
                .Include(i => i.ItemStatus)
                .OrderByDescending(i => i.Id) 
                .Take(count)
                .ToListAsync();

            foreach (var item in items)
            {
                if (item.StudioOrderId.HasValue)
                {
                    item.StudioOrder = await _orderRepository.GetAsync(item.StudioOrderId.Value);
                }
            }

            return items;
        }

        public async Task<OrderItem> GetAsync(int id)
        {
            return await _context.OrderItems
                .Include(i => i.ItemStatus)
                .Include(i => i.StudioOrder)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<OrderItem> GetAsync(OrderItem value)
        {
            return await _context.OrderItems
                .Include(i => i.ItemStatus)
                .Include(i => i.StudioOrder)
                .FirstOrDefaultAsync(i => i.Id == value.Id);
        }

        public async Task<int> Update(OrderItem item)
        {
            var existingItem = await _context.OrderItems
                .Include(o => o.ItemStatus)
                .FirstOrDefaultAsync(o => o.Id == item.Id);

            if (existingItem == null)
                throw new InvalidOperationException($"OrderItem с Id={item.Id} не найден");

            existingItem.ItemId = item.ItemId;
            existingItem.Quantity = item.Quantity;
            existingItem.Comment = item.Comment;
            existingItem.StudioOrderId = item.StudioOrderId;
            existingItem.OrderItemCode = item.OrderItemCode;
            existingItem.OrderCartId = item.OrderCartId;
            if (item.ItemStatus != null && !string.IsNullOrWhiteSpace(item.ItemStatus.Name))
            {
                var normalizedStatusName = item.ItemStatus.Name.Trim().ToLower();
                var existingStatus = await _context.ItemStatuses
                    .FirstOrDefaultAsync(s => s.Name.Trim().ToLower() == normalizedStatusName);

                if (existingStatus == null)
                {
                    existingStatus = new ItemStatus { Name = item.ItemStatus.Name.Trim() };
                    _context.ItemStatuses.Add(existingStatus); 
                }

                existingItem.ItemStatus = existingStatus;
            }
            else if (item.ItemStatusId.HasValue && item.ItemStatusId != existingItem.ItemStatusId)
            {
                existingItem.ItemStatusId = item.ItemStatusId;
            }

            return await _context.SaveChangesAsync();
        }
        public async Task<bool> SwapStudioOrderIdsWithValidationAsync(int firstOrderItemId, int secondOrderItemId)
        {
            var firstItem = await _context.OrderItems
                .Include(i => i.StudioOrder)
                .FirstOrDefaultAsync(i => i.StudioOrderId == firstOrderItemId);
    
            var secondItem = await _context.OrderItems
                .Include(i => i.StudioOrder)
                .FirstOrDefaultAsync(i => i.StudioOrderId == secondOrderItemId);

            if (firstItem == null || secondItem == null)
                return false;
            
            var tempStudioOrderId = firstItem.StudioOrderId;
            firstItem.StudioOrderId = secondItem.StudioOrderId;
            secondItem.StudioOrderId = tempStudioOrderId;

            var result = await _context.SaveChangesAsync();
            return result > 0;
        }
        public List<ItemStatus> GetCartItemStatuses()
        {
            return _context.ItemStatuses
                .Include(s => s.OrderItems) 
                .Include(s => s.StatusColor) 
                .ToList();
        }

        public List<StatusColor> GetStatusColors()
        {
            return _context.StatusColors.ToList();
        }
        
        public void UpdateStatusColor(int statusId, string colorCode)
        {
            var status = _context.ItemStatuses
                .Include(s => s.StatusColor)
                .FirstOrDefault(s => s.Id == statusId);

            if (status == null) return;

            if (status.StatusColor != null)
            {
                status.StatusColor.ColorCode = colorCode;
            }
            else
            {
                var newColor = new StatusColor { ColorCode = colorCode };
                _context.StatusColors.Add(newColor);
                status.StatusColor = newColor;
            }
    
            _context.SaveChanges();
        }

        public void ResetStatusColor(int statusId)
        {
            var status = _context.ItemStatuses.Find(statusId);
            if (status != null)
            {
                status.StatusColorId = null;
                _context.SaveChanges();
            }
        }
}