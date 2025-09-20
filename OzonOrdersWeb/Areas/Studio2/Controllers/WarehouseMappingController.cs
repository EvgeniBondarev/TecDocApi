using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using OzonDomains.Models;
using Servcies.DataServcies;

namespace OzonOrdersWeb.Areas.Studio2.Controllers;

[Area("Studio2")]
[Authorize(Roles = "User,Admin")]
public class WarehouseMappingController : Controller
{
    private readonly WarehouseMappingDataServcies _service;
    private readonly WarehouseDataServcies _warehouseDataServcies;
    private readonly OzonClientServcies _clientDataServcies; // Добавляем сервис клиентов

    public WarehouseMappingController(WarehouseMappingDataServcies service, 
                                      WarehouseDataServcies warehouseDataServcies,
                                      OzonClientServcies clientDataServcies) // Добавляем в конструктор
    {
        _service = service;
        _warehouseDataServcies = warehouseDataServcies;
        _clientDataServcies = clientDataServcies;
    }

    public async Task<IActionResult> Index()
    {
        var mappings = await _service.GetWarehouseMappings();
        return View(mappings);
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.Warehouses = new SelectList((await _warehouseDataServcies.GetWarehouses()).OrderBy(a => a.Name), "Name", "Name");
        ViewBag.Clients = new SelectList(await _clientDataServcies.GetOzonClients(), "Id", "Name"); // Добавляем список клиентов
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(WarehouseMapping model)
    {
        if (ModelState.IsValid)
        {
            await _service.AddWarehouseMapping(model);
            TempData["SuccessMessage"] = "Связь складов успешно создана!";
            return RedirectToAction(nameof(Index));
        }
        
        // Если модель невалидна, перезагружаем списки
        ViewBag.Warehouses = new SelectList((await _warehouseDataServcies.GetWarehouses()).OrderBy(a => a.Name), "Name", "Name");
        ViewBag.Clients = new SelectList(await _clientDataServcies.GetOzonClients(), "Id", "Name");
        return View(model);
    }
    
    public async Task<IActionResult> Details(int id)
    {
        var mapping = await _service.GetWarehouseMappingAsync(id);
        if (mapping == null)
            return NotFound();

        return View(mapping);
    }

    public async Task<IActionResult> Delete(int id)
    {
        var mapping = await _service.GetWarehouseMappingAsync(id);
        if (mapping == null)
            return NotFound();

        return View(mapping);
    }
    
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var mapping = await _service.GetWarehouseMappingAsync(id);
        if (mapping != null)
        {
            await _service.DeleteWarehouseMapping(mapping);
            TempData["SuccessMessage"] = "Связь складов удалена!";
        }
        return RedirectToAction(nameof(Index));
    }
}