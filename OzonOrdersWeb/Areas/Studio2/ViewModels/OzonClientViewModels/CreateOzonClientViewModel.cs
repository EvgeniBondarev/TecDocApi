using Microsoft.AspNetCore.Mvc.Rendering;
using OzonDomains;
using OzonDomains.Models;

namespace OzonOrdersWeb.ViewModels.OzonClientViewModels
{
    public class CreateOzonClientViewModel
    {
        public OzonClient OzonClient { get; set; } = new OzonClient();
       
        public List<SelectListItem> CurrencyCodes { get; set; }
        public List<SelectListItem> ClientTypes { get; set; }
        public string OzonClienResult { get; set; }
        public List<OzonClient> OzonClients { get; set; }
        public List<YandexClient> YandexClients { get; set;}
        public string YandexApiKey { get; set; }
    }
}
