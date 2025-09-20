using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OzonDomains.Models;
using OzonDomains.ViewModels;
using Servcies.DataServcies;
using Servcies.FiltersServcies.DataFilterManagers;
using Servcies.FiltersServcies.FilterModels;
using Servcies.FiltersServcies.SortModels;

namespace OzonOrdersWeb.Controllers
{
    [Authorize(Roles = "Admin")]
    [Area("Studio2")]
    public class ColumnMappingsController : Controller, ISortLogicContriller<ColumnMapping, ColumnMappingSortState>
    {
        private readonly ColumnMappingDataServcies _columnMappingDataServcies;
        private readonly ColumnMappingDataFilterManager _columnMappingDataFilterManager;

        public ColumnMappingsController(ColumnMappingDataServcies columnMappingDataServcies,
                                        ColumnMappingDataFilterManager columnMappingDataFilterManager)
        {
            _columnMappingDataServcies = columnMappingDataServcies;
            _columnMappingDataFilterManager = columnMappingDataFilterManager;
        }

        public async Task<IActionResult> Index(ColumnMappingSortState sortOrder, int page = 1)
        {
            SaveSortStateCookie(sortOrder);

            List<ColumnMapping> columnMappings = await _columnMappingDataServcies.GetColumnMappings();

            var filterDataString = HttpContext.Request.Cookies["ColumnMappingFilterData"];

            var filterData = new ColumnMappingFilterModel();
            if (!string.IsNullOrEmpty(filterDataString))
            {
                filterData = JsonConvert.DeserializeObject<ColumnMappingFilterModel>(filterDataString);
                columnMappings = _columnMappingDataFilterManager.FilterByFilterData(columnMappings, filterData);
            }

            SetSortOrderViewData(sortOrder);
            columnMappings = (await ApplySortOrder(columnMappings, sortOrder)).ToList();

            return (View(new PageViewModel<ColumnMapping, ColumnMappingFilterModel>(
                        columnMappings,
                        page,
                        15,
                        filterData)));
        }

        [HttpPost]
        public async Task<IActionResult> Index(ColumnMappingFilterModel columnMappingFilterModel, int page = 1)
        {
            List<ColumnMapping> columnMappings = await _columnMappingDataServcies.GetColumnMappings();

            columnMappings = _columnMappingDataFilterManager.FilterByFilterData(columnMappings, columnMappingFilterModel);

            var serializedFilterData = JsonConvert.SerializeObject(columnMappingFilterModel);
            HttpContext.Response.Cookies.Append("ColumnMappingFilterData", serializedFilterData);

            return (View(new PageViewModel<ColumnMapping, ColumnMappingFilterModel>(
                        columnMappings,
                        page,
                        15,
                        columnMappingFilterModel)));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var columnMapping = await _columnMappingDataServcies.GetColumnMappingAsync(id);
            if (columnMapping != null)
            {
                await _columnMappingDataServcies.DeleteColumnMapping(columnMapping);
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, ColumnMapping columnMapping)
        {
            if (id != columnMapping.Id)
            {
                return NotFound();
            }

            try
            {
                await _columnMappingDataServcies.UpdateColumnMapping(columnMapping);

            }
            catch (DbUpdateConcurrencyException)
            {
                if (await _columnMappingDataServcies.UpdateColumnMapping(columnMapping) == null)
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

        public void SetSortOrderViewData(ColumnMappingSortState sortState)
        {
            ViewData["MappingNameSort"] = sortState == ColumnMappingSortState.MappingNameAsc ? ColumnMappingSortState.MappingNameDesc : ColumnMappingSortState.MappingNameAsc;
        }

        private ColumnMappingSortState GetSortStateCookie()
        {
            var sortStateCookie = Request.Cookies["ColumnMappingSortState"];
            if (!string.IsNullOrEmpty(sortStateCookie) && Enum.TryParse<ColumnMappingSortState>(sortStateCookie, out var savedSortState))
            {
                return savedSortState;
            }
            return ColumnMappingSortState.StandardState;
        }

        public async Task<IEnumerable<ColumnMapping>> ApplySortOrder(IEnumerable<ColumnMapping> items, ColumnMappingSortState sortState)
        {
            return sortState switch
            {
                ColumnMappingSortState.MappingNameAsc => items.OrderBy(o => o.MappingName),
                ColumnMappingSortState.MappingNameDesc => items.OrderByDescending(o => o.MappingName),

                _ => items
            };
        }

        public void SaveSortStateCookie(ColumnMappingSortState sortState)
        {
            if (sortState != ColumnMappingSortState.StandardState)
            {
                Response.Cookies.Delete("ColumnMappingSortState");
                Response.Cookies.Append("ColumnMappingSortState", sortState.ToString());
            }
        }

        public async Task<IActionResult> DelSortStateCookie()
        {
            Response.Cookies.Delete("ColumnMappingSortState");
            Response.Cookies.Append("ColumnMappingSortState", ColumnMappingSortState.StandardState.ToString());
            return RedirectToAction("Index");
        }

    }
}
