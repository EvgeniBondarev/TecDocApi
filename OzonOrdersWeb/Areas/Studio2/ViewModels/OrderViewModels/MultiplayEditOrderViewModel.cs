using Microsoft.AspNetCore.Mvc.Rendering;
using OzonDomains.Models;
using OzonRepositories.Context.Identity;

namespace OzonOrdersWeb.ViewModels.OrderViewModels
{
    public class MultiplayEditOrderViewModel
    {
        public List<Order> Orders { get; set; }
        public List<Supplier> Suppliers { get; set; }
        public int RedirectPage { get; set; }

        public CustomIdentityUser User { get; set; }

        public decimal RateUSD { get; set; }
        public decimal RateEUR { get; set;}
        public decimal RateBYN { get; set; }
        public string? ErrorMessage { get; set; }

        public Dictionary<string, int> UniqueArticles { get; set; }
        public Dictionary<string, int> UniqueDeliveryCitys { get; set; }
        public Dictionary<string, int> UniqueNumbers { get; set; }
        
        public AppStatus AppStatus { get; set; }

    }
}
