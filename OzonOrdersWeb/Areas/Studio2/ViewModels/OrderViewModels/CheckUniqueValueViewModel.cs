namespace OzonOrdersWeb.ViewModels.OrderViewModels
{
    public class CheckUniqueValueViewModel
    {
        public List<string> NewOzonClients { get; set; }
        public List<string> NewStatuses { get; set; }
        public List<string> NewWarehouses { get; set; }

        public Dictionary<string, bool> NewOzonClientsDict { get; set; } = new Dictionary<string, bool>();
        public Dictionary<string, bool> NewStatusesDict { get; set; } = new Dictionary<string, bool>();
        public Dictionary<string, bool> NewWarehousesDict { get; set; } = new Dictionary<string, bool>();
    }
}
