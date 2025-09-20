using Microsoft.AspNetCore.Mvc.Rendering;
using OzonDomains.Models;

namespace OzonOrdersWeb.ViewModels.SupplierViewModels
{
    public class EditSupplierViewModel
    {
        public Supplier Supplier { get; set; }
        public List<SelectListItem> CurrencyCodes { get; set; }
    }
}
