using Microsoft.AspNetCore.Mvc;

namespace OzonOrdersWeb.Areas.PartsInfo.Controllers;

[Area("PartsInfo")]
public class ImagesController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}