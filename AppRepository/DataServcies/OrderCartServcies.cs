using OzonDomains;
using OzonDomains.Models;
using OzonDomains.Models.OrderCarts;
using OzonDomains.Models.OrderCarts.Cart;
using OzonRepositories.Data;
using Servcies.ApiServcies.TradesoftApi;
using Servcies.ApiServcies.TradesoftApi.Models;
using Servcies.Builders;
using Servcies.CacheServcies.Cache.OrderSummaryCache;
using Servcies.SignalRServcies;

namespace Servcies.DataServcies;

public class OrderCartServcies : IDataServcies
{
    private readonly OrderCartRepository _orderCartRepository;
    private readonly OrdersDataServcies _ordersDataServcies;
    private readonly MaxiPartsConfig _maxiPartsConfig;
    private readonly TradesoftDataManager _dataManager;
    private readonly OrderItemRepository _orderItemRepository;
    private readonly IOrderSummaryCache _orderSummaryCache;

    
    public static readonly Dictionary<string, string> Statuses = new()
    {
        { "MakeOrderError", "Заказ не создан" },
        { "MakeOrderProgress", "Заказ в стадии оформления" }
    };
    public OrderCartServcies(OrderCartRepository orderCartRepository,
                             OrdersDataServcies ordersDataServcies,
                             MaxiPartsConfig maxiPartsConfig,
                             TradesoftDataManager dataManager,
                             OrderItemRepository orderItemRepository,
                             IOrderSummaryCache orderSummaryCache)
    {
        _orderCartRepository = orderCartRepository;
        _ordersDataServcies = ordersDataServcies;
        _maxiPartsConfig = maxiPartsConfig;
        _dataManager = dataManager;
        _orderItemRepository = orderItemRepository;
        _orderSummaryCache = orderSummaryCache;
    }
    
    public async Task<List<int>> CreateCartItem(List<StockItem>? selectedStocks)
    {
        List<OrderItem> orderItems = new List<OrderItem>();
        var ordersToUpdate = new List<Order>();

        foreach (var item in selectedStocks)
        {
            Order orderToCart = await _ordersDataServcies.GetOrder(item.OrderId);
            OrderItem newOrderItem = new OrderItem()
            {
                ItemId = item.StokId,
                Quantity = orderToCart.Quantity.Value,
                StudioOrderId = orderToCart.Id,
                StudioOrder = orderToCart,
                ItemStatus = new ItemStatus()
                {
                    Name = "Добавлен в корзину"
                }
            };
            orderItems.Add(newOrderItem);
            ordersToUpdate.Add(orderToCart);
        }

        OrderCart newCart = new OrderCart()
        {
            Provider = "maxi_parts",
            OrderItems = orderItems,
            CartStatus = new CartStatus()
            {
                Name = "Новый элемент"
            }
        };

        var result = await _orderCartRepository.Add(newCart);

        foreach (var order in ordersToUpdate)
        {
            var correspondingItem = orderItems.FirstOrDefault(oi => oi.StudioOrderId == order.Id);
            if (correspondingItem != null)
            {
                order.CartItemId = correspondingItem.Id;
            }
        }
        await _ordersDataServcies.SaveChanges();
        var failedOrderIds = await ProcessMaxiPartsOrderAsync(newCart.OrderItems.ToList(), newCart.Id);

        foreach (var failedId in failedOrderIds)
        {
            var failedItem = newCart.OrderItems.FirstOrDefault(oi => oi.StudioOrderId == failedId);
            if (failedItem != null)
            {
                await _orderItemRepository.Delete(failedItem);
            }
        }

        return failedOrderIds;
    }
    
   public async Task<List<int>> ProcessMaxiPartsOrderAsync(List<OrderItem> cartItems, int newCartId)
   {
        var failedOrderIds = new List<int>();
        var comment = $"Заказ maxi parts {DateTime.UtcNow}, кол {cartItems.Count}";
        var summaryBuilder = new OrderSummaryTableBuilder();

        List<MakeOrderOfflineItem> makeOrderOfflineItems = new List<MakeOrderOfflineItem>();
        foreach (var cartItem in cartItems)
        {
            makeOrderOfflineItems.Add(new MakeOrderOfflineItem
            {
                ItemId = cartItem.ItemId,
                Quantity = cartItem.Quantity.ToString(),
                Reference = $"{cartItem.StudioOrder?.Id.ToString()}/{DateTime.Now.Ticks.ToString()}",
                Comment = comment
            });
        }

        var request = new MakeOrderOfflineParam
        {
            Provider = "war_provider_maxi_parts",
            Login = _maxiPartsConfig.User,
            Password = _maxiPartsConfig.Password,
            Comment = comment,
            ClientOrderNumber = DateTime.Now.Ticks.ToString(),
            Items = makeOrderOfflineItems,
        };

        var result = await _dataManager.MakeOrderOfflineAsync(request);

        if (string.IsNullOrWhiteSpace(result.Error))
        {
            var resultItems = result.Result[0].Items;
            var orderStatus = Statuses[result.Result[0].OrderStatus];

            foreach (var resultItem in resultItems)
            {
                var cartItem = cartItems.FirstOrDefault(x => x.ItemId == resultItem.ItemId);
                if (cartItem == null)
                {
                    await NotificationService.NotifyAllAsync($"Не найден элемент корзины для ItemId: {resultItem.ItemId}");
                    continue;
                }

                string shipmentNumber = cartItem.StudioOrder?.ShipmentNumber ?? "N/A";

                if (string.IsNullOrWhiteSpace(resultItem.Error))
                {
                    cartItem.OrderItemCode = resultItem.OrderItemId;
                    cartItem.ItemStatus = new ItemStatus
                    {
                        Name = orderStatus
                    };

                    await _orderItemRepository.Update(cartItem);
                    summaryBuilder.AddRow(shipmentNumber, true, orderStatus);
                }
                else
                {
                    failedOrderIds.Add(cartItem.StudioOrder.Id);
                    summaryBuilder.AddRow(shipmentNumber, false, resultItem.Error);
                }
            }
        }
        else
        {
            await NotificationService.NotifyAllAsync($"Ошибка при заказе MaxiParts: {result.Error}");

            foreach (var item in cartItems)
            {
                string shipmentNumber = item.StudioOrder?.ShipmentNumber ?? "N/A";
                summaryBuilder.AddRow(shipmentNumber, false, result.Error);
                failedOrderIds.Add(item.StudioOrder.Id);
            }
        }
        var htmlTable = summaryBuilder.Build();
        await NotificationService.NotifyAllAsync(htmlTable); 
        _orderSummaryCache.Set(cartId: newCartId, htmlTable);

        return failedOrderIds;
   }

    public List<ItemStatus> GetCartItemStatuses()
    {
       return _orderItemRepository.GetCartItemStatuses();
    }
   
    public List<StatusColor> GetStatusColors()
    {
       return _orderItemRepository.GetStatusColors();
    }

    public void UpdateStatusColor(int statusId, string colorCode)
    {
        _orderItemRepository.UpdateStatusColor(statusId, colorCode);
    }
    public Task<int> SaveChanges()
    {
        throw new NotImplementedException();
    }

    public void ResetStatusColor(int statusId)
    {
        _orderItemRepository.ResetStatusColor(statusId);
    }
}