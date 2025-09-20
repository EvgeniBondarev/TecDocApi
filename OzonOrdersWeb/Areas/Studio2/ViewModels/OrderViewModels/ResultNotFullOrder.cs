using OzonDomains.Models;

namespace OzonOrdersWeb.ViewModels.OrderViewModels
{
    public class ResultNotFullOrder
    {
        public Order Order { get; set; }
        public List<int>? OrderIds { get; set; } 
        public bool Hidden { get; set; }    
    }
}
