using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OzonDomains.Models;
using OzonOrdersWeb.ViewModels.AppStatusViewModels;
using Servcies.CacheServcies.Cache.OzonOrdersCache;
using Servcies.DataServcies;
using Servcies.FiltersServcies.DataFilterManagers;
using Servcies.FiltersServcies.FilterModels;
using Servcies.FiltersServcies.SortModels;
using Services.CacheServcies.Cache;
using Services.CacheServcies.Cache.OzonOrdersCache;

namespace OzonOrdersWeb.Controllers
{
    [Authorize(Roles = "Admin")]
    [Area("Studio2")]
    public class AppStatusController : Controller, ISortLogicContriller<AppStatus, AppStatusSortState>
    {
        private readonly AppStatusDataServcies _servcies;
        private readonly OrdersDataServcies _ordersDataServcies;
        private readonly OrderCache _cache;
        private readonly CacheUpdater<Order> _cacheUpdater;
        private readonly AppStatusDataFilterManager _dataFilterManager;
        public AppStatusController(AppStatusDataServcies servcies,
                                   OrdersDataServcies ordersDataServcies,
                                   OrderCache orderCache,
                                   CacheUpdater<Order> cacheUpdater,
                                   AppStatusDataFilterManager appStatusDataFilter)
        {
            _servcies = servcies;
            _ordersDataServcies = ordersDataServcies;
            _cache = orderCache;
            _cacheUpdater = cacheUpdater;
            _dataFilterManager = appStatusDataFilter;
        }

        public async Task<IActionResult> Index(AppStatusSortState sortOrder = AppStatusSortState.StandardState, int page = 1)
        {
            SaveSortStateCookie(sortOrder);

            List<AppStatus> appStatuses = await _servcies.GetAppStatuses();

            var filterDataString = HttpContext.Request.Cookies["AppStatusFilterData"];

            var filterData = new AppStatusFilterModel();
            if (!string.IsNullOrEmpty(filterDataString))
            {
                filterData = JsonConvert.DeserializeObject<AppStatusFilterModel>(filterDataString);
                if (filterData != null) appStatuses = _dataFilterManager.FilterByFilterData(appStatuses, filterData);
            }

            SetSortOrderViewData(sortOrder);
            appStatuses = (await ApplySortOrder(appStatuses, sortOrder)).ToList();

            var appStatusViewModel = new AppStatusViewModel<AppStatus, AppStatusFilterModel>(appStatuses, page, 15, filterData);

            return View(appStatusViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Index(AppStatusFilterModel filterData, int page = 1)
        {
            List<AppStatus> appStatuses = await _servcies.GetAppStatuses();
            appStatuses = _dataFilterManager.FilterByFilterData(appStatuses, filterData);

            var serializedFilterData = JsonConvert.SerializeObject(filterData);
            HttpContext.Response.Cookies.Append("AppStatusFilterData", serializedFilterData);

            return View(new AppStatusViewModel<AppStatus, AppStatusFilterModel>(appStatuses, page, 15, filterData)
            {
            });
        }

        public async Task<IActionResult> Create()
        {
            string result = (string)TempData["StatusResult"];
            ViewData["StatusResult"] = result;

            var appStatusList = await _servcies.GetAppStatuses();
            ViewData["AppStatusList"] = appStatusList;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create([Bind("Id,Name")] AppStatus appStatus)
        {
            if (ModelState.IsValid)
            {


                var existingStatus = await _servcies.GetAppStatusAsync(appStatus);
                if (existingStatus != null)
                {
                    TempData["StatusResult"] = $"Не удалось добавить статус '{appStatus.Name}'";
                    return RedirectToAction(nameof(Create));
                }

                await _servcies.AddAppStatus(appStatus);

                return RedirectToAction("Index");

            }
            return View(appStatus);
        }


        public async Task<IActionResult> Delete(int? id)
        {
            var appStatus = await _servcies.GetAppStatusAsync(id.Value);
            if (appStatus != null)
            {
                var defaultStatus = await _servcies.GetAppStatusAsync(new AppStatus(){Name = "Не указан"});

                var ordersWithDeletedStatus = (await _ordersDataServcies.GetOrders()).ToList();
                ordersWithDeletedStatus = ordersWithDeletedStatus.Where(o => o.AppStatusId == id).ToList();

                foreach (var order in ordersWithDeletedStatus)
                {
                    order.AppStatus = defaultStatus;
                }
                await _servcies.DeleteAppStatus(appStatus);
                await _cacheUpdater.Update(_cache);
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(int id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appStatus = await _servcies.GetAppStatusAsync(id);
            if (appStatus == null)
            {
                return NotFound();
            }

            return View(appStatus);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AppStatus appStatus)
        {
            if (id != appStatus.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _servcies.UpdateAppStatus(appStatus);
                    await _cacheUpdater.Update(_cache);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (await _servcies.GetAppStatusAsync(id) == null)
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(appStatus);
        }
        
        [HttpPost]
        public async Task<IActionResult> UpdateStatusColor(int statusId, string colorCode)
        {
            try
            {
                await _servcies.UpdateStatusColor(statusId, colorCode);
                await _cacheUpdater.Update(_cache);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ResetStatusColor(int statusId)
        {
            try
            {
                await _servcies.ResetStatusColor(statusId);
                await _cacheUpdater.Update(_cache);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }


        public void SetSortOrderViewData(AppStatusSortState appStatusSort)
        {
            ViewData["AppStatusSortNameSort"] = appStatusSort == AppStatusSortState.NameAsc ? AppStatusSortState.NameDesc : AppStatusSortState.NameAsc;

        }

        public async Task<IEnumerable<AppStatus>> ApplySortOrder(IEnumerable<AppStatus> appStatuses, AppStatusSortState appStatusSort)
        {
            return appStatusSort switch
            {
                AppStatusSortState.NameAsc => appStatuses.OrderBy(o => o.Name),
                AppStatusSortState.NameDesc => appStatuses.OrderByDescending(o => o.Name),

                _ => appStatuses
            }; ;
        }

        public void SaveSortStateCookie(AppStatusSortState appStatusSortState)
        {
            if (appStatusSortState != AppStatusSortState.StandardState)
            {
                Response.Cookies.Delete("AppStatusSortState");
                Response.Cookies.Append("AppStatusSortState", appStatusSortState.ToString());
            }
        }

        [HttpPost]
        public async Task<IActionResult> DelSortStateCookie()
        {
            Response.Cookies.Delete("AppStatusSortState");
            Response.Cookies.Append("AppStatusSortState", AppStatusSortState.StandardState.ToString());
            return RedirectToAction("Index");
        }
    }
}
