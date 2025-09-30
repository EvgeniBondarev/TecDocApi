using Microsoft.EntityFrameworkCore;
using OzonDomains.Models;
using OzonRepositories.Context;
using OzonRepositories.Data;
using Servcies.DataServcies.DTO;
using Servcies.PriceСlculationServcies;
using System.Reflection;
using System.Text.RegularExpressions;
using OzonDomains;

namespace Servcies.DataServcies
{
    public class OrdersDataServcies(
        OrderRepository repository,
        OzonOrderContext context,
        OrderPriceManager orderPriceManager,
        AppStatusDataServcies appStatusDataServcies,
        SupplierDataServcies supplierDataServcies,
        ProductsDataServcies productsDataServcies,
        WarehouseDataServcies warehouseDataServcies,
        ExcludedArticleDataServcies excludedArticleDataServcies)
        : IDataServcies
    {
        private readonly string[] _ignoredProperties =
        [
            "AppStatus",  "СurrencyId", "ProductInfoId", "UpdatedColumns", "Id", "AppStatusId", "ShipmentWarehouseId",
            "IsVerified", "Comment", "Key", "IsAccepted", "MaxCommissionInfo", "MinCommissionInfo", "MinDiscount",
            "MaxDiscount", "PurchasePrice","Supplier", "SupplierId", "MinProfit", "MaxProfit", "UpdatedBy", "IsReturnable",
            "OrderNumberToSupplier", "FromFile", "OriginalPurchasePrice", "CostPrice", "PurchasePrice"
        ];

        private readonly string[] _ignoredWebProperties =
        [
            "СurrencyId", "ProductInfoId", "UpdatedColumns", "Id", "ShipmentWarehouseId",
            "ShipmentWarehouse", "Сurrency", "ProductInfo", "MaxCommissionInfo", "MinCommissionInfo",
             "IsVerified", "Key", "IsAccepted", "OzonClient", "FromFile"
        ];
        
        private readonly string[] _ignoredWebNullProperties =
        [
            "OrderNumberToSupplier", "Supplier",  "SupplierId"
        ];
        
        private readonly string[] _priceProperties =
        [
            "ShipmentAmount",
            "Price",
            "PurchasePrice",
            "OriginalPurchasePrice",
            "MinOzonCommission",
            "MaxOzonCommission",
            "MinProfit",
            "MaxProfit",
            "MinDiscount",
            "MaxDiscount",
            "CostPrice"
        ];
        
        public async Task<IQueryable<Order>> GetOrders()
        {
            var result = await repository.GetAsync();

            return result;
        }

        public async Task<IQueryable<Order>> GetByLastMonthsForFilterAsync(int months)
        {
            return await repository.GetByLastMonthsAsync(months);
        }

        public async Task<Order> GetOrder(int id)
        {
            var result = await repository.GetAsync(id);

            return result;
        }

        public async Task<Order> GetOrder(Order order)
        {
            var result = await repository.GetAsync(order);

            return result;
        }

        public async Task<int> UpdateOrder(Order order)
        {
            return await repository.Update(order);
        }
        public async Task<int> UpdateOrders(List<Order> orders)
        {
            return await repository.UpdateRange(orders);
        }

        public async Task<IQueryable<Order>> GetOrders(int skip, int take)
        {
            return await repository.GetOrdersWithPagination(skip, take);
        }

        public async Task<int> GetTotalOrderCount()
        {
            return await repository.GetTotalOrderCount();
        }

        public async Task<int> GetReturnableCount()
        {
            return await repository.GetReturnableCount();
        }

        public async Task<IQueryable<Order>> GetOrdersByManufacturerCode(string code)
        {
            return await repository.GetOrdersByManufacturerCode(code);
        }

        public async Task<int> DeleteOrder(int orderId)
        {
            Order orderToDelete = await repository.GetAsync(orderId);

            if(orderToDelete != null && orderToDelete.AppStatus.Name == "Не указан")
            {
                return await repository.Delete(orderToDelete);
            }
            return 0;
        }

        public async Task<Dictionary<string, int>> GetUniqueArticles()
        {
            var result = await repository.GetUniqueValues(o => o.ProductKey);
            return result.ToDictionary(kv => kv.Key, kv => kv.Value);
        }

        public async Task<Dictionary<string, int>> GetUniqueDeliveryCities()
        {
            var result = await repository.GetUniqueValues(o => o.DeliveryCity);
            return result.ToDictionary(kv => kv.Key, kv => kv.Value);
        }

        public async Task<Dictionary<string, int>> GetUniqueShipmentNumbers()
        {
            var shipmentNumbers = await repository.GetShipmentNumbers();
            var result = shipmentNumbers
                .Select(sn => sn.Split('-')[0])
                .GroupBy(sn => sn)
                .ToDictionary(g => g.Key, g => g.Count());

            return result;
        }

        public async Task<int> MultiplayDeleteOrders(int[] idArray)
        {
            int result = 0;
            
            foreach(int orderId in idArray)
            {
                Order orderToDelete = await repository.GetAsync(orderId);

                if (orderToDelete != null && orderToDelete.AppStatus.Name == "Не указан")
                {
                    result += await repository.Delete(orderToDelete);
                }
            }

            return result;
        }

        public async Task<int[]> AddOrders(List<Order> orders)
        {
            orders = await excludedArticleDataServcies.ExcludeOrdersAsync(orders);
            if (orders.Count == 0)
                return [0, 0];
            await using (var transaction = await context.Database.BeginTransactionAsync())
            {
                var chunkSize = 500;
                var updateCount = 0;
                var addCount = 0;
                var ordersToInsert = new List<Order>();
                var status = context.AppStatuses.FirstOrDefault(s => s.Name == "Не указан");

                for (int i = 0; i < orders.Count; i += chunkSize)
                {
                    var currentChunk = orders.Skip(i).Take(chunkSize).ToList();
                    foreach (var order in currentChunk)
                    {
                        Order existingOrder;
                        
                        if (order.OzonClient?.ClientType == ClientType.YANDEX)
                        {
                            existingOrder = context.Orders.FirstOrDefault(o => o.Key == order.Key);
                            bool endsWithSlashNumber = existingOrder != null &&
                                                       Regex.IsMatch(existingOrder.ShipmentNumber, @"/\d+$");
                            if (endsWithSlashNumber)
                            {
                                continue;
                            }
                            
                        }
                        else
                        {
                            var trimmedShipmentNumber = TrimShipmentNumber(order.ShipmentNumber);
    
                            var existingOrders = context.Orders
                                .Where(o => o.Article == order.Article)
                                .AsEnumerable()
                                .Where(o => TrimShipmentNumber(o.ShipmentNumber) == trimmedShipmentNumber)
                                .ToList();

                            var endsWithSlashNumber = existingOrders
                                .FirstOrDefault(o => !string.IsNullOrEmpty(o.ShipmentNumber) && Regex.IsMatch(o.ShipmentNumber, @"/\d+$"));

                            if (endsWithSlashNumber != null)
                            {
                                continue;
                            }
                        
                            existingOrder = existingOrders.FirstOrDefault(eo => eo.ShipmentNumber == order.ShipmentNumber);
                        }
                        
                        try
                        {
                            if (existingOrder != null)
                            {
                                await UpdateOrder(existingOrder, order);
                                updateCount++;
                                    
                            }
                            else
                            {
                                ordersToInsert.Add(await CastToModel(order));
                                addCount++;
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            continue;
                        }
                    }
                    await context.Orders.AddRangeAsync(ordersToInsert);
                    await context.SaveChangesAsync();
                    ordersToInsert.Clear();
                }

                await transaction.CommitAsync();

                return [addCount, updateCount];
            }

        }
        
        public async Task<int> MultiplayEditOrder(IEnumerable<Order> orders)
        {
            foreach(Order order in orders)
            {
                try
                {
                    Order complitOrder = await CastToModel(order);
                    Order existingOrder = await GetOrder(complitOrder.Id);
                    await UpdateOrderWeb(existingOrder, complitOrder);
                    existingOrder.IsVerified = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Не удалось изменить заказ {order.ShipmentNumber}");
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                    Console.WriteLine(ex.Data);
                    
                    throw new Exception(message: $"Не удалось изменить заказ: {ex.Message}");
                }
            }
            return await SaveChanges();
        }    
        public async Task<Order> TransactOrder(Order order)
        {
            var complitOrder = await CastToModel(order);
            var existingOrder = await GetOrder(complitOrder.Id);
            await UpdateOrderWeb(existingOrder, complitOrder);
            existingOrder.IsVerified = true;

            await SaveChanges();
            return existingOrder;
        }
        private async Task UpdateOrderWeb(Order existingOrder, Order jsonOrder)
        {
            var isVerified = existingOrder.IsVerified;
            var isIsAccepted = existingOrder.IsAccepted;

            var orderChangeList = GetOrderChangeList(existingOrder, jsonOrder, _ignoredWebProperties);

            if (orderChangeList.Any())
            {
                existingOrder.UpdatedColumns = orderChangeList;
            }

            if (existingOrder.UpdatedColumns != null)
            {
                var properties = typeof(Order).GetProperties();

                foreach (var prop in properties)
                {
                    try
                    {
                        var jsonProp = jsonOrder.GetType().GetProperty(prop.Name);
                        var existingProp = existingOrder.GetType().GetProperty(prop.Name);

                        if (jsonProp == null || existingProp == null || !existingProp.CanWrite)
                            return;

                        var value = jsonProp.GetValue(jsonOrder);
                        
                        bool shouldUpdate = 
                            value != null && prop.Name != "Id" && existingOrder.UpdatedColumns.Contains(prop.Name) ||
                            value == null && _ignoredWebNullProperties.Contains(prop.Name);

                        if (shouldUpdate)
                        {
                            existingProp.SetValue(existingOrder, value);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error setting property {prop.Name}: {ex.Message}");
                    }
                }
            }

            if (existingOrder.UpdatedColumns != null && existingOrder.UpdatedColumns.Contains("AppStatusId"))
            {
                existingOrder.LastStatusChangeDate = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time"));
            }
            if (existingOrder.UpdatedColumns != null && !existingOrder.UpdatedColumns.Contains("IsAccepted"))
            {
                existingOrder.IsAccepted = false;
            }
            else
            {
                existingOrder.IsAccepted = isIsAccepted;
            }

            existingOrder.IsVerified = isVerified;
        }
        
        private async Task UpdateOrder(Order existingOrder, Order jsonOrder)
        {
            var isVerified = existingOrder.IsVerified;
            var isIsAccepted = existingOrder.IsAccepted;

            if (isVerified)
            {

            }

            var orderChangeList = GetOrderChangeList(existingOrder, jsonOrder, _ignoredProperties);

            if (orderChangeList.Any())
            {
                existingOrder.UpdatedColumns = orderChangeList;
            }
            else
            {
                existingOrder.UpdatedColumns = null;
            }

            if (existingOrder.UpdatedColumns != null)
            {
                var properties = typeof(Order).GetProperties();
                var jsonOrderType = jsonOrder.GetType();

                foreach (var prop in properties)
                {
                    try
                    {
                        var jsonOrderProp = jsonOrderType.GetProperty(prop.Name);

                        if (jsonOrderProp != null)
                        {
                            var o = jsonOrderProp.GetValue(jsonOrder);

                            if (o != null && prop.Name != "Id" && existingOrder.UpdatedColumns.Contains(prop.Name))
                            {
                                // 🔒 проверка: не даём обновлять ценовые поля, если они уже заданы
                                if (_priceProperties.Contains(prop.Name))
                                {
                                    var currentValue = prop.GetValue(existingOrder);
                                    if (currentValue != null) 
                                    {
                                        continue; // пропускаем обновление этого свойства
                                    }
                                }

                                var existingOrderProp = existingOrder.GetType().GetProperty(prop.Name);
                                if (existingOrderProp != null && existingOrderProp.CanWrite)
                                {
                                    existingOrderProp.SetValue(existingOrder, o);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error setting property {prop.Name}: {ex.Message}");
                    }
                }
            }

            if (existingOrder.UpdatedColumns != null && existingOrder.UpdatedColumns.Contains("AppStatusId"))
            {
                existingOrder.LastStatusChangeDate = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time"));
            }

            if (existingOrder.UpdatedColumns != null && existingOrder.UpdatedColumns[0] != "IsAccepted")
            {
                existingOrder.IsAccepted = false;
            }
            else
            {
                existingOrder.IsAccepted = isIsAccepted;
            }

            existingOrder.IsVerified = isVerified;

            //await CastToModel(existingOrder);
        }


        private List<string> GetOrderChangeList(Order existingOrder, Order jsonOrder, string[] ignoredProperties)
        {
            List<string> changeProp = new List<string>();

            changeProp = CompareObjects(existingOrder, jsonOrder, changeProp, ignoredProperties);

            return changeProp;
        }

        private List<string> CompareObjects(object obj1, object obj2, List<string> changeProp, string[] ignoredProperties)
        {
            Type type = obj1.GetType();

            foreach (PropertyInfo property in type.GetProperties())
            {
                if (property.CanRead)
                {
                    if (ignoredProperties.Contains(property.Name))
                    {
                        continue;
                    }

                    object val1 = property.GetValue(obj1) == "" ? null : property.GetValue(obj1);
                    object val2 = property.GetValue(obj2) == "" ? null : property.GetValue(obj2);

                    if (val1 != null && val2 != null)
                    {
                        if (property.PropertyType.Assembly == type.Assembly && !property.PropertyType.IsPrimitive && property.PropertyType != typeof(string))
                        {
                            CompareObjects(val1, val2, changeProp, ignoredProperties);
                        }
                        else if (!val1.Equals(val2))
                        {
                            changeProp.Add(property.Name);
                        }
                    }
                    else if (val1 != null || val2 != null)
                    {
                        changeProp.Add(property.Name);
                    }
                }
            }
            return changeProp;
        }


        public async Task ConfirmAccepted(int id)
        {
            var order = await repository.GetAsync(id);

            order.IsAccepted = true;
            order.IsVerified = true;
            order.UpdatedColumns = null;

            await SaveChanges();
        }
        
        private async Task<Order> CastToModel(Order order)
        {
            if (order.AppStatus == null && order.AppStatusId != null)
            {
                
                if (order.AppStatusId.HasValue)
                {
                    order.AppStatus = await appStatusDataServcies.GetAppStatusAsync(order.AppStatusId.Value);
                }

                if (order.SupplierId.HasValue)
                {
                    order.Supplier = await supplierDataServcies.GetSupplierAsync(order.SupplierId.Value);
                }

                decimal? weight = order.ProductInfo?.Weight;

                if (order.ProductInfoId.HasValue)
                {
                    order.ProductInfo = await productsDataServcies.GetProductAsync(order.ProductInfoId.Value);
                }

                order.ProductInfo.Weight = weight;
                await productsDataServcies.Update(order.ProductInfo);

                order = await orderPriceManager.SetPurchasePriceToRUB(order);

                order = await orderPriceManager.CalculateCostPrice(order);

                order = await orderPriceManager.CalculateProfit(order);
                order = await orderPriceManager.CalculateDiscount(order);
            }
            order = await SetIsReturnable(order);

            return order;
        }
        
        private async Task<Order> CastToModelForOzon(Order order)
        {
            if (order.AppStatus == null && order.AppStatusId != null)
            {
                
                if (order.AppStatusId.HasValue)
                {
                    order.AppStatus = await appStatusDataServcies.GetAppStatusAsync(order.AppStatusId.Value);
                }

                if (order.SupplierId.HasValue)
                {
                    order.Supplier = await supplierDataServcies.GetSupplierAsync(order.SupplierId.Value);
                }

                decimal? weight = order.ProductInfo?.Weight;

                if (order.ProductInfoId.HasValue)
                {
                    order.ProductInfo = await productsDataServcies.GetProductAsync(order.ProductInfoId.Value);
                }

                order.ProductInfo.Weight = weight;
                await productsDataServcies.Update(order.ProductInfo);

                //order = await orderPriceManager.SetPurchasePriceToRUB(order);

                //order = await orderPriceManager.CalculateCostPrice(order);

                //order = await orderPriceManager.CalculateProfit(order);
                //order = await orderPriceManager.CalculateDiscount(order);
            }
            order = await SetIsReturnable(order);

            return order;
        }

        private async Task<Order> SetIsReturnable(Order order)
        {
            if (order.AppStatus != null)
            {
                if (order.AppStatus.Name == "Заказан поставщику" && order.Status == "Отменён")
                {
                    order.IsReturnable = true;
                }
                else
                {
                    order.IsReturnable = false;
                }
            }
            return order;
        }

        public async Task<List<Order>> CalculateCostPriceForNotFullOrders(List<Order> orders)
        {
            List<Order> result = new List<Order>();
    
            foreach (var order in orders)
            {
                try
                {
                    var reformedOrder = await orderPriceManager.SetPurchasePriceToRUB(order);
                    var calculatedOrder = await orderPriceManager.CalculateCostPrice(reformedOrder);
                    result.Add(calculatedOrder);
                }
                catch
                {
                    result.Add(order); // добавляем исходный order при ошибке
                }
            }

            return result;
        }

        public NotFullOrdersModel GetNotFullOrdersModel(List<Order> orders)
        {
            // Преобразование ProductKey на клиентской стороне
            var productKeys = new HashSet<string>(orders.Select(o => o.ProductKey.Split('=')[0]));

            // Получаем все заказы с сервера
            var allAppOrders = repository.Get().ToList();

            // Фильтруем заказы на клиентской стороне
            var appOrders = allAppOrders.Where(o => productKeys.Contains(o.ProductKey.Split('=')[0])).ToList();

            // Коллекция для отслеживания использованных заказов
            var usedOrders = new HashSet<Order>();

            // Словарь для группировки заказов по ProductKey.Split('=')[0]
            var groupedOrders = orders.GroupBy(order => order.ProductKey.Split('=')[0]).ToDictionary(g => g.Key, g => g.ToList());

            // Создаем словари для результатов
            var uniqueOrders = new List<Order>();
            var ordersWithMultipleMatches = new Dictionary<Order, List<Order>>();
            var ordersWithOneMatch = new Dictionary<Order, Order>();

            foreach (var article in groupedOrders.Keys)
            {
                var orderGroup = groupedOrders[article];
                var matchedOrders = appOrders.Where(o => o.ProductKey.Split('=')[0] == article).ToList();
                
                if (matchedOrders.Count == 0 || matchedOrders.All(o => o.Status == "Отменен" || o.Status == "Отменён"))
                {
                    uniqueOrders.AddRange(orderGroup);
                    continue;
                }

                if (matchedOrders.Count > 1)
                {
                    var filteredMatches = matchedOrders
                        .Where(repoOrder => !usedOrders.Contains(repoOrder) && 
                                            repoOrder.Status != "Отменен" && 
                                            repoOrder.Status != "Отменён")
                        .ToList();

                    if (filteredMatches.Any())
                    {
                        foreach (var order in orderGroup)
                        {
                            ordersWithMultipleMatches[order] = filteredMatches;
                            usedOrders.UnionWith(filteredMatches);
                        }
                    }
                }
                else if (matchedOrders.Count == 1)
                {
                    var matchedOrder = matchedOrders.First();
                    if (matchedOrder.Status != "Отменен" && matchedOrder.Status != "Отменён" && 
                        !usedOrders.Contains(matchedOrder))
                    {
                        foreach (var order in orderGroup)
                        {
                            ordersWithOneMatch[order] = matchedOrder;
                            usedOrders.Add(matchedOrder);
                        }
                    }
                }
            }
            
            uniqueOrders = uniqueOrders
                .Where(o => !usedOrders.Contains(o))
                .OrderBy(o => o.ProductKey.Split('=')[0])
                .ToList();

            return new NotFullOrdersModel
            {
                UniqueOrders = uniqueOrders,
                OrdersWithMultipleMatches = ordersWithMultipleMatches.OrderBy(pair => pair.Key.ProductKey.Split('=')[0]).ToDictionary(pair => pair.Key, pair => pair.Value),
                OrdersWithOneMatches = ordersWithOneMatch.OrderBy(pair => pair.Key.ProductKey.Split('=')[0]).ToDictionary(pair => pair.Key, pair => pair.Value)
            };
        }

        public async Task<List<Order>> ProcessingNotFullOrder(Order notFullOrder, List<int> orderIds, string updatedBy)
        {
            List<Order> result = new List<Order>();
            bool isNewOrder = orderIds == null || !orderIds.Any();
            notFullOrder = await CastToFullOrder(notFullOrder, isNewOrder);

            if (isNewOrder)
            {
                result.Add(await PrepareNewOrder(notFullOrder, updatedBy));
            }
            else
            {
                result = await ProcessExistingOrders(notFullOrder, orderIds, updatedBy);
            }

            return result;
        }

        public async Task<List<Order>> ProcessingNotFullOrderForExcel(Order notFullOrder, List<int> orderIds)
        {
            bool isNewOrder = orderIds == null || !orderIds.Any();
            notFullOrder = await CastToFullOrder(notFullOrder, isNewOrder);

            return isNewOrder
                ? new List<Order> { notFullOrder }
                : await ProcessOrdersForExcel(notFullOrder, orderIds);
        }

        private async Task<Order> PrepareNewOrder(Order order, string updatedBy)
        {
            order = await CastToModel(order);
            order.UpdatedBy = updatedBy;
            order.LastStatusChangeDate = GetRussianCurrentTime();
            return order;
        }

        private async Task<List<Order>> ProcessExistingOrders(Order templateOrder, List<int> orderIds, string updatedBy)
        {
            var distinctOrderIds = orderIds.Distinct().ToList();
            List<Order> result = new List<Order>();

            foreach (var id in distinctOrderIds)
            {
                var order = await repository.GetAsync(id);
                if (order == null) continue;

                UpdateOrderProperties(order, templateOrder, updatedBy);
                order = await orderPriceManager.CalculateProfit(order);
                order = await orderPriceManager.CalculateDiscount(order);
                result.Add(order);
            }

            return result;
        }

        private async Task<List<Order>> ProcessOrdersForExcel(Order templateOrder, List<int> orderIds)
        {
            var distinctOrderIds = orderIds.Distinct().ToList();
            List<Order> ordersToExcel = new List<Order>();

            foreach (var id in distinctOrderIds)
            {
                var order = await repository.GetAsync(id);
                if (order == null) continue;

                UpdateOrderProperties(order, templateOrder);
                order = await orderPriceManager.CalculateProfit(order);
                order = await orderPriceManager.CalculateDiscount(order);
                ordersToExcel.Add(order);
            }

            return ordersToExcel;
        }

        private void UpdateOrderProperties(Order targetOrder, Order sourceOrder, string updatedBy = null)
        {
            if (targetOrder.AppStatus != sourceOrder.AppStatus)
            {
                targetOrder.LastStatusChangeDate = GetRussianCurrentTime();
            }

            targetOrder.AppStatus = sourceOrder.AppStatus;
            targetOrder.PurchasePrice = sourceOrder.PurchasePrice;
            targetOrder.CostPrice = sourceOrder.CostPrice;
            if (!string.IsNullOrEmpty(updatedBy))
            {
                targetOrder.UpdatedBy = updatedBy;
            }
        }

        private DateTime GetRussianCurrentTime()
        {
            return TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time"));
        }

        private async Task<Order> CastToFullOrder(Order notFullOrder, bool isNewOrder)
        {
            notFullOrder.AppStatus = await GetAppStatusAsync(notFullOrder.AppStatus?.Id);
            notFullOrder.Manufacturer = GetManufacturerById(notFullOrder.Manufacturer?.Id);
            notFullOrder.ShipmentWarehouse = await GetWarehouseAsync(notFullOrder.ShipmentWarehouse?.Id);
            notFullOrder.Supplier = await GetSupplierAsync(notFullOrder.SupplierId ?? 0);
            notFullOrder.ProductInfo = await GetProductAsync(notFullOrder.ProductInfo?.Id);
            notFullOrder.OzonClient = GetOzonClientById(notFullOrder.OzonClient?.Id);

            if (isNewOrder)
            {
                notFullOrder = await orderPriceManager.CalculateProfit(notFullOrder);
                notFullOrder = await orderPriceManager.CalculateDiscount(notFullOrder);
            }

            return notFullOrder;
        }

        private async Task<AppStatus> GetAppStatusAsync(int? id)
        {
            return id != null ? await appStatusDataServcies.GetAppStatusAsync(id.Value) : null;
        }

        private Manufacturer GetManufacturerById(int? id)
        {
            return id != null ? context.Manufacturers.FirstOrDefault(m => m.Id == id.Value) : null;
        }

        private async Task<Warehouse> GetWarehouseAsync(int? id)
        {
            return id != null ? await warehouseDataServcies.GetWarehouseAsync(id.Value) : null;
        }

        private async Task<Supplier> GetSupplierAsync(int id)
        {
            return id != 0 ? await supplierDataServcies.GetSupplierAsync(id) : null;
        }

        private async Task<Product> GetProductAsync(int? id)
        {
            return id != null ? await productsDataServcies.GetProductAsync(id.Value) : null;
        }

        private OzonClient GetOzonClientById(int? id)
        {
            return id != null ? context.OzonClients.FirstOrDefault(o => o.Id == id.Value) : null;
        }

        public async Task<List<Order>> SetNumberInExcel(List<Order> orders)
        {
            int numberInExcel = 2;
            foreach (var order in orders)
            {
                order.NumberInExcel = numberInExcel++;
            }
            return orders;
        }


        public async Task<List<Order>> SetUniqueShipmentNumberAndKey(List<Order> orders)
        {
            var shipmentNumberCounts = new Dictionary<string, int>();

            foreach (var order in orders)
            {
                if (!string.IsNullOrEmpty(order.ShipmentNumber))
                {
                    if (shipmentNumberCounts.ContainsKey(order.ShipmentNumber))
                    {
                        shipmentNumberCounts[order.ShipmentNumber]++;
                        int number = shipmentNumberCounts[order.ShipmentNumber];
                        order.ShipmentNumber = $"{order.ShipmentNumber}/{number}";
                        order.Key = $"{order.Key}/{number}";
                    }
                    else
                    {
                        shipmentNumberCounts[order.ShipmentNumber] = 1;
                    }
                }
            }
            return orders;
        }

        public async Task<int> CancellationOrders(int[] idArray)
        {
            var appStatus = await context.AppStatuses.FirstOrDefaultAsync(a => a.Name == "Отменен");

            if (appStatus == null)
            {
                AppStatus newStatus = new AppStatus()
                {
                    Name = "Отменен"
                };
                context.AppStatuses.Add(newStatus);
                await context.SaveChangesAsync();
            }

            foreach (int orderId in idArray)
            {
                Order orderToCancellation = await repository.GetAsync(orderId);

                if (orderToCancellation != null &&
                    IsStatusAllowedForCancellation(orderToCancellation.AppStatus?.Name) &&
                    (orderToCancellation.Status == "Отменён" || string.IsNullOrEmpty(orderToCancellation.Status)))
                {
                    orderToCancellation.AppStatus = appStatus;
                    orderToCancellation.LastStatusChangeDate = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time"));
                }
            }
            return await SaveChanges();
        }
        private static bool IsStatusAllowedForCancellation(string statusName)
        {
            return statusName == "Не указан" || statusName == "Приостановлен";
        }
        
        public List<Order> MergeOrders(List<Order> orders)
        {
            return orders
                .GroupBy(o => new { o.Article, o.EtProducer })
                .Select(g =>
                {
                    int? totalQuantity = g.Sum(o => o.Quantity);
                    decimal? totalPurchasePrice = g.Sum(o => o.PurchasePrice * o.Quantity);
                    var firstOrder = g.First();

                    if (totalQuantity == 0 || !totalQuantity.HasValue)
                    {
                        return null;
                    }

                    var mergedOrder = new Order
                    {
                        Article = g.Key.Article,
                        EtProducer = g.Key.EtProducer,
                        PurchasePrice = Math.Round((decimal)(totalPurchasePrice.Value / totalQuantity.Value), 2),
                        Quantity = totalQuantity,
                    };

                    foreach (PropertyInfo property in typeof(Order).GetProperties())
                    {
                        if (property.Name == nameof(Order.Article) ||
                            property.Name == nameof(Order.EtProducer) ||
                            property.Name == nameof(Order.PurchasePrice) ||
                            property.Name == nameof(Order.Quantity))
                        {
                            continue;
                        }
                        property.SetValue(mergedOrder, property.GetValue(firstOrder));
                    }

                    return mergedOrder;
                })
                .Where(order => order != null) 
                .OrderBy(o => o.Article)
                .ThenBy(o => o.EtProducer?.Name)
                .ToList();
        }

        public async Task<int> SaveChanges()
        {
            return await repository.SaveChanges();
        }

        public async Task SplitOrder(int orderId, int part1, int part2)
        {
            Order order = await repository.GetAsync(orderId);
            if (order.Quantity > 1)
            {
                if (part1 + part2 != order.Quantity.Value)
                {
                    throw new ArgumentException("Сумма частей должна равняться исходному количеству");
                }
        
                order.Quantity = part1;
        
                var newOrder = order.Clone();
                newOrder.Quantity = part2;
                newOrder.ShipmentNumber = order.ShipmentNumber + "/2";
                order.ShipmentNumber = order.ShipmentNumber + "/1";

                await UpdateOrder(order);
                await context.Orders.AddAsync(newOrder);
                await context.SaveChangesAsync();
            }
        }

        
        private string TrimShipmentNumber(string? shipmentNumber)
        {
            if (string.IsNullOrEmpty(shipmentNumber))
                return shipmentNumber ?? string.Empty;

            var trimmed = shipmentNumber.Trim();

            // Удаление всех окончаний вида /число (например, /1, /2 и т.п.)
            trimmed = Regex.Replace(trimmed, @"(/[\d]+)+$", string.Empty);

            // Удаление окончания вида -X, где X — одна цифра
            if (trimmed.Length >= 2 && trimmed[^2] == '-' && char.IsDigit(trimmed[^1]))
            {
                trimmed = trimmed[..^2];
            }

            return trimmed;
        }

        public async Task<List<int>> GetOrderIdsByNumbersAndArticles(List<(string OrderNumber, string Article)> orders)
        {
            return await repository.GetOrderIdsByNumbersAndArticles(orders);
        }
   
        
        public async Task<int> GetTotalCount()
        {
            return await repository.GetTotalCount();
        }
    }
}
