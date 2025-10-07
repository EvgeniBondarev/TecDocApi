using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using OzonDomains.Models;
using OzonOrdersWeb.Areas.Studio2.ViewModels.Warehouse;
using OzonOrdersWeb.Controllers;
using Servcies.DataServcies;
using Servcies.FiltersServcies.FilterModels;
using Servcies.FiltersServcies.SortModels;

namespace OzonOrdersWeb.Areas.Studio2.Controllers;

[Area("Studio2")]
[Authorize(Roles = "User,Admin")]
public class WarehouseMappingController : Controller, ISortLogicContriller<WarehouseMapping, WarehouseMappingSortState>
{
    private readonly WarehouseMappingDataServcies _service;
    private readonly WarehouseDataServcies _warehouseDataServcies;
    private readonly OzonClientServcies _clientDataServcies;

    public WarehouseMappingController(WarehouseMappingDataServcies service, 
                                      WarehouseDataServcies warehouseDataServcies,
                                      OzonClientServcies clientDataServcies)
    {
        _service = service;
        _warehouseDataServcies = warehouseDataServcies;
        _clientDataServcies = clientDataServcies;
    }

    public async Task<IActionResult> Index(WarehouseMappingSortState sortOrder = WarehouseMappingSortState.StandardState, int page = 1)
    {
        SaveSortStateCookie(sortOrder);

        List<WarehouseMapping> mappings = await _service.GetWarehouseMappings();

        var filterDataString = HttpContext.Request.Cookies["WarehouseMappingFilterData"];

        var filterData = new WarehouseMappingFilterModel();
        if (!string.IsNullOrEmpty(filterDataString))
        {
            filterData = JsonConvert.DeserializeObject<WarehouseMappingFilterModel>(filterDataString);
            mappings = FilterByFilterData(mappings, filterData);
        }

        SetSortOrderViewData(sortOrder);
        mappings = (await ApplySortOrder(mappings, sortOrder)).ToList();

        // Загружаем список клиентов для выпадающего списка
        ViewBag.Clients = new SelectList(await _clientDataServcies.GetOzonClients(), "Id", "Name");

        var viewModel = new WarehouseMappingViewModel<WarehouseMapping, WarehouseMappingFilterModel>(
            mappings, page, 15, filterData);

        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> Index(WarehouseMappingFilterModel filterData, int page = 1)
    {
        List<WarehouseMapping> mappings = await _service.GetWarehouseMappings();
        mappings = FilterByFilterData(mappings, filterData);

        var serializedFilterData = JsonConvert.SerializeObject(filterData);
        HttpContext.Response.Cookies.Append("WarehouseMappingFilterData", serializedFilterData);

        // Загружаем список клиентов для выпадающего списка
        ViewBag.Clients = new SelectList(await _clientDataServcies.GetOzonClients(), "Id", "Name");

        return View(new WarehouseMappingViewModel<WarehouseMapping, WarehouseMappingFilterModel>(
            mappings, page, 15, filterData));
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.Warehouses = new SelectList((await _warehouseDataServcies.GetWarehouses()).OrderBy(a => a.Name), "Name", "Name");
        ViewBag.Clients = new SelectList(await _clientDataServcies.GetOzonClients(), "Id", "Name");
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
        
        ViewBag.Warehouses = new SelectList((await _warehouseDataServcies.GetWarehouses()).OrderBy(a => a.Name), "Name", "Name");
        ViewBag.Clients = new SelectList(await _clientDataServcies.GetOzonClients(), "Id", "Name");
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(WarehouseMapping model)
    {
        if (ModelState.IsValid)
        {
            try
            {
                await _service.UpdateWarehouseMapping(model);
                TempData["SuccessMessage"] = "Связь складов успешно обновлена!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Ошибка при обновлении: {ex.Message}";
            }
        }
        else
        {
            TempData["ErrorMessage"] = "Ошибка валидации данных";
        }

        return RedirectToAction(nameof(Index));
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

    // Реализация интерфейса ISortLogicContriller
    public void SetSortOrderViewData(WarehouseMappingSortState sortOrder)
    {
        ViewData["IdSort"] = sortOrder == WarehouseMappingSortState.IdAsc ? WarehouseMappingSortState.IdDesc : WarehouseMappingSortState.IdAsc;
        ViewData["ClientNameSort"] = sortOrder == WarehouseMappingSortState.ClientNameAsc ? WarehouseMappingSortState.ClientNameDesc : WarehouseMappingSortState.ClientNameAsc;
        ViewData["BitrixWarehouseSort"] = sortOrder == WarehouseMappingSortState.BitrixWarehouseAsc ? WarehouseMappingSortState.BitrixWarehouseDesc : WarehouseMappingSortState.BitrixWarehouseAsc;
        ViewData["OzonWarehouseSort"] = sortOrder == WarehouseMappingSortState.OzonWarehouseAsc ? WarehouseMappingSortState.OzonWarehouseDesc : WarehouseMappingSortState.OzonWarehouseAsc;
    }

    public async Task<IEnumerable<WarehouseMapping>> ApplySortOrder(IEnumerable<WarehouseMapping> mappings, WarehouseMappingSortState sortOrder)
    {
        return sortOrder switch
        {
            WarehouseMappingSortState.IdAsc => mappings.OrderBy(o => o.Id),
            WarehouseMappingSortState.IdDesc => mappings.OrderByDescending(o => o.Id),

            WarehouseMappingSortState.ClientNameAsc => mappings.OrderBy(o => o.OzonClient?.Name ?? ""),
            WarehouseMappingSortState.ClientNameDesc => mappings.OrderByDescending(o => o.OzonClient?.Name ?? ""),

            WarehouseMappingSortState.BitrixWarehouseAsc => mappings.OrderBy(o => o.BitrixWarehouseName),
            WarehouseMappingSortState.BitrixWarehouseDesc => mappings.OrderByDescending(o => o.BitrixWarehouseName),

            WarehouseMappingSortState.OzonWarehouseAsc => mappings.OrderBy(o => o.OzonWarehouseName),
            WarehouseMappingSortState.OzonWarehouseDesc => mappings.OrderByDescending(o => o.OzonWarehouseName),

            _ => mappings
        };
    }

    public void SaveSortStateCookie(WarehouseMappingSortState sortOrder)
    {
        if (sortOrder != WarehouseMappingSortState.StandardState)
        {
            Response.Cookies.Delete("WarehouseMappingSortState");
            Response.Cookies.Append("WarehouseMappingSortState", sortOrder.ToString());
        }
    }

    [HttpPost]
    public async Task<IActionResult> DelSortStateCookie()
    {
        Response.Cookies.Delete("WarehouseMappingSortState");
        Response.Cookies.Append("WarehouseMappingSortState", WarehouseMappingSortState.StandardState.ToString());
        return RedirectToAction("Index");
    }

    // Метод фильтрации
    private List<WarehouseMapping> FilterByFilterData(List<WarehouseMapping> mappings, WarehouseMappingFilterModel filterData)
    {
        if (!string.IsNullOrEmpty(filterData.ClientName))
        {
            mappings = mappings.Where(m => 
                m.OzonClient?.Name?.Contains(filterData.ClientName, StringComparison.OrdinalIgnoreCase) == true)
                .ToList();
        }

        if (!string.IsNullOrEmpty(filterData.BitrixWarehouseName))
        {
            mappings = mappings.Where(m => 
                m.BitrixWarehouseName?.Contains(filterData.BitrixWarehouseName, StringComparison.OrdinalIgnoreCase) == true)
                .ToList();
        }

        if (!string.IsNullOrEmpty(filterData.OzonWarehouseName))
        {
            mappings = mappings.Where(m => 
                m.OzonWarehouseName?.Contains(filterData.OzonWarehouseName, StringComparison.OrdinalIgnoreCase) == true)
                .ToList();
        }

        return mappings;
    }
}