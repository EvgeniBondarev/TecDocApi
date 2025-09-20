using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OzonOrdersWeb.ViewModels.OrderViewModels;
using Servcies.DataServcies;

namespace OzonOrdersWeb.Areas.Studio2.Controllers;

[Authorize(Roles = "User,Admin")]
[Area("Studio2")]
public class OrderCartItemStatusController : Controller
{
    private readonly OrderCartServcies _orderCartServcies;

    public OrderCartItemStatusController(OrderCartServcies orderCartServcies)
    {
        _orderCartServcies = orderCartServcies;
    }
    
    public IActionResult CartStatuses(int? highlightId = null)
    {
        var result = _orderCartServcies.GetCartItemStatuses();
        ViewBag.HighlightId = highlightId;
    
        return View(result);
    }

    [HttpPost]
    public IActionResult UpdateStatusColor(int statusId, string colorCode)
    {
        _orderCartServcies.UpdateStatusColor(statusId, colorCode);
        return Json(new { success = true });
    }
    [HttpPost]
    public IActionResult ResetStatusColor(int statusId)
    {
        _orderCartServcies.ResetStatusColor(statusId);
        return Json(new { success = true });
    }
    
}