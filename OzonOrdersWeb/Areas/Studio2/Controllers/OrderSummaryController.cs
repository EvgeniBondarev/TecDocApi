using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Servcies.CacheServcies.Cache.OrderSummaryCache;

namespace OzonOrdersWeb.Areas.Studio2.Controllers;

[Authorize(Roles = "Admin")]
[Area("Studio2")]
public class OrderSummaryController : Controller
{
    private readonly IOrderSummaryCache _orderSummaryCache;

    public OrderSummaryController(IOrderSummaryCache orderSummaryCache)
    {
        _orderSummaryCache = orderSummaryCache;
    }

    [HttpGet]
    public IActionResult ViewSummary(int cartId)
    {
        var htmlTable = _orderSummaryCache.Get(cartId);
        if (string.IsNullOrWhiteSpace(htmlTable))
        {
            return Content(
                $"Сводка по заказу с ID {cartId} не найдена или устарела.",
                "text/plain",
                Encoding.UTF8
            );
        }
        return View("OrderSummary", model: htmlTable);
    }
}