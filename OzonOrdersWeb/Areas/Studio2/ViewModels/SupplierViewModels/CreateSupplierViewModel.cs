using Microsoft.AspNetCore.Mvc.Rendering;
using OzonDomains.Models;

namespace OzonOrdersWeb.ViewModels.SupplierViewModels
{
    public class CreateSupplierViewModel
    {
        public Supplier Supplier { get; set; } = new Supplier();
        public List<SelectListItem> CurrencyCodes { get; set; }
        public string SupplierResult { get; set; }
        public List<Supplier> SuppliersList { get; set; }
    }
}
