using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TreeGrouping.Web.Controllers;

[Area("OzonCards")]
[Authorize(Roles = "User,Admin")]
public class TasksController: Controller
{
    public IActionResult Index()
    {
        return View();
    }
}