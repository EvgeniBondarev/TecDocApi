using OzonDomains.Models;

namespace OzonOrdersWeb.ViewModels.OrderViewModels
{
    public class NotFullOrdersViewModel
    {
        public List<Order> UniqueOrders { get; set; }
        public Dictionary<Order ,List<Order>> OrdersWithMultipleMatches { get; set; }
        public Dictionary<Order, Order> OrdersWithOneMatches { get; set; }
        public List<OzonClient> OzonClients { get; set; }
        public List<Supplier> Suppliers { get; set; }
        public List<AppStatus> AppStatuses { get; set; }    
        public List<Manufacturer> Manufacturers { get; set; }
        public List<Warehouse> Warehouse { get; set; }
        public AppStatus? SelectedAppStatus { get; set; }
    }
}
