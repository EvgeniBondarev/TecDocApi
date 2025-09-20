using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Servcies.SignalRServcies;

[Authorize(Roles = "User,Admin")]
[Area("Studio2")]
public class OrderHistoryController : Controller
{
    private readonly IOrderHistoryDataService _orderHistoryService;
    private readonly IHubContext<OrderHistoryHub> _hubContext;

    public OrderHistoryController(IOrderHistoryDataService orderHistoryService, IHubContext<OrderHistoryHub> hubContext)
    {
        _orderHistoryService = orderHistoryService;
        _hubContext = hubContext;
    }

    public async Task<IActionResult> Index()
    {
        var recentChanges = await _orderHistoryService.GetRecentChangesAsync(100);
        return View(recentChanges);
    }

    [HttpGet]
    public async Task<IActionResult> GetRecentChanges()
    {
        var changes = await _orderHistoryService.GetRecentChangesAsync(50);
        return Ok(changes);
    }
    
    [HttpGet]
    public async Task<IActionResult> GetOrderHistory(int orderId)
    {
        var histories = await _orderHistoryService.GetByOrderIdAsync(orderId);

        if (histories == null || histories.Count == 0)
            return NotFound($"История по заказу {orderId} не найдена.");
        var order = histories.First().Order;

        var viewModel = new OrderHistoryViewModel
        {
            OrderId = orderId,
            ShipmentNumber = order?.ShipmentNumber,
            ProductName = order?.ProductName,
            Article = order?.Article,
            ClientName = order?.OzonClient?.Name,
            Histories = histories
                .GroupBy(h => h.ChangedAt.Date)
                .OrderByDescending(g => g.Key) // последние даты сверху
                .ToDictionary(g => g.Key, g => g.ToList())
        };

        return View(viewModel);
    }
}