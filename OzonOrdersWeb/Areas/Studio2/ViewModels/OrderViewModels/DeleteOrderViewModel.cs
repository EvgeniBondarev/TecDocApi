using OzonDomains.Models;

namespace OzonOrdersWeb.ViewModels.OrderViewModels
{
    public class DeleteOrderViewModel
    {
        public List<Order> OrdersToDelete { get; set; }
        public List<int> IdsToDelete { get; set; }
    }
}
