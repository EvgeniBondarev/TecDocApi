using OzonDomains.Models;
using OzonRepositories.Context.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OzonOrdersWeb.ViewModels.OrderViewModels
{ 
    public class EditOrderViewModel
    {
        public Order Order { get; set; }
        public List<Supplier> Suppliers { get; set; }

        public CustomIdentityUser User { get; set; }

        public int RedirectPage { get; set; }

        public decimal RateUSD { get; set; }
        public decimal RateEUR { get; set; }
        public decimal RateBYN { get; set; }

        public string? ErrorMessage { get; set; }
    }
}
