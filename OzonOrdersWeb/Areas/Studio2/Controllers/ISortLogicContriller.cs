using Microsoft.AspNetCore.Mvc;
using OServcies.FiltersServcies.FilterModels;
using OzonDomains.Models;
using Servcies.FiltersServcies.SortModels;

namespace OzonOrdersWeb.Controllers
{
    public interface ISortLogicContriller<T, K>
    {
        public Task<IActionResult> Index(K sortOrder, int page);

        public void SetSortOrderViewData(K sortState);

        public Task<IEnumerable<T>> ApplySortOrder(IEnumerable<T> items, K sortState);

        public void SaveSortStateCookie(K sortState);

        public Task<IActionResult> DelSortStateCookie();
    }
}
