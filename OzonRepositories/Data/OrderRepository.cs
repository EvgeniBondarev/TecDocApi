using Microsoft.EntityFrameworkCore;
using OzonDomains.Models;
using OzonRepositories.Context;
using System.Linq.Expressions;
using OzonDomains.Models.OrderCarts;

namespace OzonRepositories.Data
{
    public class OrderRepository : MainRepository
    {
        private readonly OzonOrderContext _context;
        private readonly JcEtalonContext _jcEtalonContext;
        public OrderRepository(OzonOrderContext context, JcEtalonContext jcEtalonContext) : base(context)
        {
            _context = context;
            _jcEtalonContext = jcEtalonContext; 
        }

        public async Task<int> Add(Order value)
        {
            await _context.Orders.AddAsync(value);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> Delete(Order value)
        {
            _context.Orders.Remove(value);
            return await _context.SaveChangesAsync();
        }
        
        public IQueryable<Order> Get()
        {
            var orders = _context.Orders
                .Include(c => c.Сurrency)
                .Include(w => w.ShipmentWarehouse)
                .Include(p => p.ProductInfo)
                .Include(a => a.AppStatus)
                .ThenInclude(s => s.StatusColor)
                .Include(s => s.Supplier)
                .Include(o => o.OzonClient)
                .Include(m => m.Manufacturer)
                .Include(f => f.ExcelFileData)
                .Include(d => d.Delivery)         
                .ThenInclude(dp => dp.Provider) 
                .OrderByDescending(o => o.ProcessingDate);
    
            var orderList = orders.ToList();
            LoadAdditionalData(orderList);

            return orderList.AsQueryable();
        }
        
        public async Task<IQueryable<Order>> GetAsync()
        {
            var orders = _context.Orders
                .Include(c => c.Сurrency)
                .Include(w => w.ShipmentWarehouse)
                .Include(p => p.ProductInfo)
                .Include(a => a.AppStatus)
                .ThenInclude(s => s.StatusColor)
                .Include(s => s.Supplier)
                .Include(o => o.OzonClient)
                .Include(m => m.Manufacturer)
                .Include(f => f.ExcelFileData)
                .Include(d => d.Delivery)         
                .ThenInclude(dp => dp.Provider)
                .OrderByDescending(o => o.ProcessingDate);
    
            var orderList = await orders.ToListAsync();
    
            // Оптимизированная загрузка EtProducers и статусов
            LoadAdditionalData(orderList);

            return orderList.AsQueryable();
        }
        
        public async Task<IQueryable<Order>> GetByLastMonthsAsync(int months)
        {
            var fromDate = DateTime.UtcNow.AddMonths(-months);

            var orders = _context.Orders
                .Include(c => c.Сurrency)
                .Include(w => w.ShipmentWarehouse)
                .Include(p => p.ProductInfo)
                .Include(a => a.AppStatus)
                .ThenInclude(s => s.StatusColor)
                .Include(s => s.Supplier)
                .Include(o => o.OzonClient)
                .Include(m => m.Manufacturer)
                .Include(f => f.ExcelFileData)
                .Include(d => d.Delivery)
                .ThenInclude(dp => dp.Provider)
                .Where(o => o.ProcessingDate >= fromDate) // фильтр по дате
                .OrderByDescending(o => o.ProcessingDate);

            var orderList = await orders.ToListAsync();
            LoadAdditionalData(orderList);

            return orderList.AsQueryable();
        }


        public async Task<Order> GetAsync(int id)
        {
            var order = await _context.Orders
                .Include(c => c.Сurrency)
                .Include(w => w.ShipmentWarehouse)
                .Include(p => p.ProductInfo)
                .Include(a => a.AppStatus)
                .ThenInclude(s => s.StatusColor)
                .Include(s => s.Supplier)
                .Include(o => o.OzonClient)
                .Include(m => m.Manufacturer)
                .Include(f => f.ExcelFileData)
                .Include(d => d.Delivery)         
                .ThenInclude(dp => dp.Provider)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return null;
            }
            
            if (order.CartItemId.HasValue)
            {
                order.CartItemStatus = _context?.OrderItems?
                    .Include(o => o.ItemStatus)
                    .FirstOrDefault(oi => oi.StudioOrderId == order.Id)?
                    .ItemStatus;
                order.CartItemStatusColor = await _context.StatusColors.FindAsync(order.CartItemStatus?.StatusColorId);
            }

            // Загрузка производителя (если есть)
            if (order.EtProducerId.HasValue)
            {
                order.EtProducer = await _jcEtalonContext.EtProducers
                    .FirstOrDefaultAsync(e => e.Id == order.EtProducerId.Value);
            }
            return order;
        }
        
        public async Task<List<int>> GetOrderIdsByNumbersAndArticles(List<(string OrderNumber, string Article)> orders)
        {
            if (orders == null || orders.Count == 0)
                return new List<int>();

            // Переводим в список для удобства поиска на клиенте
            var orderKeys = orders.ToList();

            // Сначала выбираем все заказы, которые могут соответствовать (ShipmentNumber и ProductKey)
            var allOrders = await _context.Orders
                .Where(o => orderKeys.Select(k => k.OrderNumber).Contains(o.ShipmentNumber))
                .AsNoTracking()
                .ToListAsync();

            // Фильтруем на клиенте точно по кортежу (OrderNumber + Article)
            var matchingIds = allOrders
                .Where(o => orderKeys.Any(k => k.OrderNumber == o.ShipmentNumber && k.Article == o.ProductKey))
                .Select(o => o.Id)
                .ToList();

            return matchingIds;
        }


        
        public async Task<Order> GetAsyncForCart(int id)
        {
            var orderTask = _context.Orders
                .Include(p => p.ProductInfo)
                .Include(a => a.AppStatus)
                .Include(s => s.Supplier)
                .Include(o => o.OzonClient)
                .Include(m => m.Manufacturer)
                .Include(d => d.Delivery)         
                .ThenInclude(dp => dp.Provider)
                .FirstOrDefaultAsync(o => o.Id == id);

            var order = await orderTask;
            if (order == null) return null;

            if (order.EtProducerId is int etProducerId)
            {
                order.EtProducer = await _jcEtalonContext.EtProducers
                    .FirstOrDefaultAsync(e => e.Id == etProducerId);
            }

            return order;
        }

        public async Task<Order> GetAsync(Order value)
        {
            var order = await _context.Orders
                .Include(c => c.Сurrency)
                .Include(w => w.ShipmentWarehouse)
                .Include(p => p.ProductInfo)
                .Include(a => a.AppStatus)
                .ThenInclude(s => s.StatusColor)
                .Include(s => s.Supplier)
                .Include(o => o.OzonClient)
                .Include(m => m.Manufacturer)
                .Include(f => f.ExcelFileData)
                .Include(d => d.Delivery)         
                .ThenInclude(dp => dp.Provider)
                .FirstOrDefaultAsync(o => o.Key == value.Key);

            if (order == null) return null;

            if (order.CartItemId.HasValue)
            {
                order.CartItemStatus = _context?.OrderItems?
                    .Include(o => o.ItemStatus)
                    .FirstOrDefault(oi => oi.StudioOrderId == order.Id)?
                    .ItemStatus;
                order.CartItemStatusColor = await _context.StatusColors.FindAsync(order.CartItemStatus?.StatusColorId);
            }
            
            if (order.EtProducerId.HasValue)
            {
                order.EtProducer = await _jcEtalonContext.EtProducers
                    .Where(e => e.Id == order.EtProducerId.Value)
                    .FirstOrDefaultAsync();
            }

            return order;
        }

        public async Task<IQueryable<Order>> GetOrdersByManufacturerCode(string code)
        {
            return _context.Orders.Include(m => m.Manufacturer)
                                  .Where(o => o.Manufacturer.Code == code);
        }

        public async Task<int> Update(Order value)
        {
            _context.Orders.Update(value);
            return await _context.SaveChangesAsync();
        }
        
        public async Task<int> UpdateRange(List<Order> orders)
        {
            if (orders == null || !orders.Any())
                return 0;

            _context.Orders.UpdateRange(orders);
            return await _context.SaveChangesAsync();
        }

        public async Task<IQueryable<Order>> GetOrdersWithPagination(int skip, int take)
        {
            // Основной запрос с пейджингом
            var ordersQuery = _context.Orders
                .Include(c => c.Сurrency)
                .Include(w => w.ShipmentWarehouse)
                .Include(p => p.ProductInfo)
                .Include(a => a.AppStatus)
                .ThenInclude(s => s.StatusColor) 
                .Include(s => s.Supplier)
                .Include(o => o.OzonClient)
                .Include(m => m.Manufacturer)
                .Include(f => f.ExcelFileData)
                .Include(d => d.Delivery)         
                .ThenInclude(dp => dp.Provider)
                .OrderByDescending(o => o.ProcessingDate)
                .Skip(skip)
                .Take(take);
            
            var orderList = await ordersQuery.ToListAsync();
            
            // Загрузка EtProducers
            var etProducerIds = orderList
                .Where(order => order.EtProducerId.HasValue)
                .Select(order => order.EtProducerId.Value)
                .Distinct()
                .ToList();
            
            var etProducers = new Dictionary<int, EtProducer>();
            if (etProducerIds.Any())
            {
               
                foreach (var batch in etProducerIds.Chunk(200))
                {
                    // Альтернатива 1: Использование цикла для каждого ID
                    var batchProducers = new Dictionary<int, EtProducer>();
    
                    foreach (var id in batch)
                    {
                        var producer = await _jcEtalonContext.EtProducers
                            .FirstOrDefaultAsync(p => p.Id == id);
        
                        if (producer != null)
                        {
                            batchProducers[id] = producer;
                        }
                    }
    
                    foreach (var kvp in batchProducers)
                    {
                        etProducers[kvp.Key] = kvp.Value;
                    }
                }
            }
            foreach (var order in orderList)
            {
                if (order.EtProducerId.HasValue && etProducers.TryGetValue(order.EtProducerId.Value, out var etProducer))
                {
                    order.EtProducer = etProducer;
                }

                if (order.CartItemId.HasValue)
                {
                    order.CartItemStatus = _context?.OrderItems?
                        .Include(o => o.ItemStatus)
                        .FirstOrDefault(oi => oi.StudioOrderId == order.Id)?
                        .ItemStatus;
                    order.CartItemStatusColor = await _context.StatusColors.FindAsync(order.CartItemStatus?.StatusColorId);
                }
            }

            return orderList.AsQueryable();
        }


        public async Task<int> GetTotalOrderCount()
        {
            return await _context.Orders.CountAsync();
        }

        public async Task<int> GetReturnableCount()
        {
            return await _context.Orders
                .Where(o => o.IsReturnable == true)
                .CountAsync();
        }

        public async Task<IQueryable<KeyValuePair<string, int>>> GetUniqueValues<T>(Expression<Func<Order, T>> selector, Expression<Func<Order, bool>> predicate = null) where T : class
        {
            IQueryable<Order> query = _context.Orders;

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            return query
                .Select(selector)
                .Where(value => value != null)
                .GroupBy(value => value)
                .Select(group => new KeyValuePair<string, int>(group.Key.ToString(), group.Count()))
                .AsQueryable();
        }

        public async Task<List<string>> GetShipmentNumbers()
        {
            return await _context.Orders
                                 .Where(o => o.ShipmentNumber != null)
                                 .Select(o => o.ShipmentNumber)
                                 .ToListAsync();
        }
        
        private void LoadAdditionalData(List<Order> orders)
        {
            // Загрузка EtProducers
            var etProducerIds = orders
                .Where(order => order.EtProducerId.HasValue)
                .Select(order => order.EtProducerId.Value)
                .Distinct()
                .ToList();

            var etProducers = new Dictionary<int, EtProducer>();
            var idSet = new HashSet<int>(etProducerIds);

            var allProducers = _jcEtalonContext.EtProducers
                .AsNoTracking()
                .ToList();

            foreach (var producer in allProducers)
            {
                if (idSet.Contains(producer.Id))
                {
                    etProducers[producer.Id] = producer;
                }
            }
            
            // Загрузка CartItemStatus для всех заказов одним запросом
            var orderIdsWithCartItem = orders
                .Where(order => order.CartItemId.HasValue)
                .Select(order => order.Id)
                .Distinct()
                .ToList();

            if (orderIdsWithCartItem.Any())
            {
                var orderItemsWithStatus = _context.OrderItems
                    .Include(oi => oi.ItemStatus)
                        .ThenInclude(ist => ist.StatusColor) 
                    .Where(oi => orderIdsWithCartItem.Contains(oi.StudioOrderId ?? 0))
                    .AsNoTracking() 
                    .ToList();

                var orderItemsByOrderId = orderItemsWithStatus
                    .GroupBy(oi => oi.StudioOrderId)
                    .ToDictionary(g => g.Key ?? 0, g => g.FirstOrDefault());

                foreach (var order in orders)
                {
                    if (order.EtProducerId.HasValue && etProducers.TryGetValue(order.EtProducerId.Value, out var etProducer))
                    {
                        order.EtProducer = etProducer;
                    }

                    if (order.CartItemId.HasValue)
                    {
                        if (orderItemsByOrderId.TryGetValue(order.Id, out var orderItem))
                        {
                            order.CartItemStatus = orderItem?.ItemStatus;
                            order.CartItemStatusColor = orderItem?.ItemStatus?.StatusColor;
                        }
                        else
                        {
                            order.CartItemStatus = null;
                            order.CartItemStatusColor = null;
                        }
                    }
                }
            }
            else
            {
                foreach (var order in orders)
                {
                    if (order.EtProducerId.HasValue && etProducers.TryGetValue(order.EtProducerId.Value, out var etProducer))
                    {
                        order.EtProducer = etProducer;
                    }
                }
            }
        }

        public async Task<int> GetTotalCount()
        {
            return await _context.Orders.CountAsync();
        }
    }
}
