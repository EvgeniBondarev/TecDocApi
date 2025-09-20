using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OzonDomains.Models;
using OzonDomains.ViewModels;
using OzonOrdersWeb.Controllers;
using Servcies.CacheServcies.Cache.OzonOrdersCache;
using Servcies.DataServcies;
using Servcies.FiltersServcies.DataFilterManagers;
using Servcies.FiltersServcies.FilterModels;
using Servcies.FiltersServcies.SortModels;
using Services.CacheServcies.Cache.OzonOrdersCache;

namespace OrdersApp.Controllers
{
    [Authorize(Roles = "Admin")]
    [Area("Studio2")]
    public class ManufacturersController : Controller, ISortLogicContriller<Manufacturer, ManufacturerSortState>
    {
        private readonly ManufacturerDataService _manufacturerDataService;
        private readonly OrdersDataServcies _ordersDataServcies;
        private readonly OrderCache _orderCache;
        private readonly ManufacturerFilterManager _dataFilterManager;

        public ManufacturersController(ManufacturerDataService manufacturerDataService,
                                       OrdersDataServcies ordersDataServcies,
                                       OrderCache orderCache,
                                       ManufacturerFilterManager manufacturerFilterManager)
        {
            _manufacturerDataService = manufacturerDataService;
            _ordersDataServcies = ordersDataServcies;
            _orderCache = orderCache;
            _dataFilterManager = manufacturerFilterManager;
        }

        public async Task<IActionResult> Index(ManufacturerSortState sortOrder = ManufacturerSortState.StandardState, int page = 1)
        {
            SaveSortStateCookie(sortOrder);

            List<Manufacturer> manufacturers = await _manufacturerDataService.GetManufacturers();

            var filterDataString = HttpContext.Request.Cookies["ManufacturerFilterData"];

            var filterData = new ManufacturerFilterModel();
            if (!string.IsNullOrEmpty(filterDataString))
            {
                filterData = JsonConvert.DeserializeObject<ManufacturerFilterModel>(filterDataString);
                manufacturers = _dataFilterManager.FilterByFilterData(manufacturers, filterData);
            }

            SetSortOrderViewData(sortOrder);
            manufacturers = (await ApplySortOrder(manufacturers, sortOrder)).ToList();


            return View(new PageViewModel<Manufacturer, ManufacturerFilterModel>(manufacturers, page, 15, filterData));
        }

        [HttpPost]
        public async Task<IActionResult> Index(ManufacturerFilterModel filterData, int page = 1)
        {
            List<Manufacturer> manufacturers = await _manufacturerDataService.GetManufacturers();
            manufacturers = _dataFilterManager.FilterByFilterData(manufacturers, filterData);

            var serializedFilterData = JsonConvert.SerializeObject(filterData);
            HttpContext.Response.Cookies.Append("ManufacturerFilterData", serializedFilterData);

            return View(new PageViewModel<Manufacturer, ManufacturerFilterModel>(manufacturers, page, 15, filterData));
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Code,Name")] Manufacturer manufacturer)
        {
            Manufacturer isOrig = (await _manufacturerDataService.GetManufacturers()).FirstOrDefault(m => m.Name?.ToLower() == manufacturer?.Name?.ToLower() &&
                                                                                                            m.Code?.ToLower() == manufacturer?.Code?.ToLower());
            if (isOrig == null)
            {
                if (ModelState.IsValid)
                {
                    _manufacturerDataService.AddManufacturer(manufacturer);
                    return RedirectToAction(nameof(Index));
                }
                return View(manufacturer);
            }
            else
            {
                throw new Exception();
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var manufacturer = await _manufacturerDataService.GetManufacturerAsync(id);
            if (manufacturer == null)
            {
                return NotFound();
            }
            return View(manufacturer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Code,Name")] Manufacturer manufacturer, int page = 1)
        {
            if (id != manufacturer.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                Manufacturer isOrig = (await _manufacturerDataService.GetManufacturers()).FirstOrDefault(m => m.Name?.ToLower() == manufacturer?.Name?.ToLower() &&
                                                                                                            m.Code?.ToLower() == manufacturer?.Code?.ToLower());

                try
                {
                    if (isOrig == null)
                    {
                        await _manufacturerDataService.UpdateManufacturer(manufacturer);
                        List<Order> ordersToUpdate = (await _ordersDataServcies.GetOrdersByManufacturerCode(manufacturer.Code)).ToList();
                        await _orderCache.Update();
                    }
                    else
                    {
                        throw new Exception();
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (await _manufacturerDataService.GetManufacturerAsync(id) == null)
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index), new { sortOrder = GetSortStateCookie(), page = page });
            }
            return View(manufacturer);
        }

        public void SetSortOrderViewData(ManufacturerSortState manufacturerSort)
        {
            ViewData["ManufacturerCodeSort"] = manufacturerSort == ManufacturerSortState.CodeAsc ? ManufacturerSortState.CodeDesc : ManufacturerSortState.CodeAsc;
            ViewData["ManufacturerNameSort"] = manufacturerSort == ManufacturerSortState.NameAsc ? ManufacturerSortState.NameDesc : ManufacturerSortState.NameAsc;
        }

        public async Task<IEnumerable<Manufacturer>> ApplySortOrder(IEnumerable<Manufacturer> manufacturers, ManufacturerSortState manufacturerSort)
        {
            return manufacturerSort switch
            {
                ManufacturerSortState.CodeAsc => manufacturers.OrderBy(o => o.Code),
                ManufacturerSortState.CodeDesc => manufacturers.OrderByDescending(o => o.Code),

                ManufacturerSortState.NameAsc => manufacturers.OrderBy(o => o.Name),
                ManufacturerSortState.NameDesc => manufacturers.OrderByDescending(o => o.Name),

                _ => manufacturers
            }; ;
        }

        private ManufacturerSortState GetSortStateCookie()
        {
            var sortStateCookie = Request.Cookies["ManufacturerSortState"];
            if (!string.IsNullOrEmpty(sortStateCookie) && Enum.TryParse<ManufacturerSortState>(sortStateCookie, out var savedSortState))
            {
                return savedSortState;
            }
            return ManufacturerSortState.StandardState;
        }

        public void SaveSortStateCookie(ManufacturerSortState manufacturerSort)
        {
            if (manufacturerSort != ManufacturerSortState.StandardState)
            {
                Response.Cookies.Delete("ManufacturerSortState");
                Response.Cookies.Append("ManufacturerSortState", manufacturerSort.ToString());
            }
        }

        [HttpPost]
        public async Task<IActionResult> DelSortStateCookie()
        {
            Response.Cookies.Delete("ManufacturerSortState");
            Response.Cookies.Append("ManufacturerSortState", ManufacturerSortState.StandardState.ToString());
            return RedirectToAction("Index");
        }
    }
}
