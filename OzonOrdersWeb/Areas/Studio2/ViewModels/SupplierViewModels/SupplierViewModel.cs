using Microsoft.AspNetCore.Mvc.Rendering;
using OzonDomains;
using OzonDomains.ViewModels;

namespace OzonOrdersWeb.ViewModels.SupplierViewModels
{
    public class SupplierViewModel<T, K> : PageViewModel<T, K>
    {
        public List<(CurrencyCode, string)> CurrencyCodes { get; set; }
        public List<SelectListItem> CurrencyCodesSelectList { get; set; }

        public SupplierViewModel(IEnumerable<T> pageDate, int page, int pageSize, K filterModel) : base(pageDate, page, pageSize, filterModel)
        {
        }
    }
}
