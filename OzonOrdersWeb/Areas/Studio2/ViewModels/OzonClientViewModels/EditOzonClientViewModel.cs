using Microsoft.AspNetCore.Mvc.Rendering;
using OzonDomains.Models;
using OzonOrdersWeb.Areas.Studio2.ViewModels.OzonClientViewModels;

namespace OzonOrdersWeb.ViewModels.OzonClientViewModels
{
    public class EditOzonClientViewModel
    {
        public OzonClient OzonClient { get; set; }
        public List<SelectListItem> CurrencyCodes { get; set; }
        public List<SelectListItem> ClientTypes { get; set; }
    
        public List<Role> ApiRoles { get; set; }
        public bool HasApiAccess { get; set; }
    }
}
