using OzonDomains.Models;

namespace OzonOrdersWeb.ViewModels.OrderViewModels
{
    public class FileOrderRowViewModel
    {
        public Order Order { get; set; }
        public List<AppStatus> AppStatuses { get; set; }
        public List<OzonClient> OzonClients { get; set; }
        public int NumberInExcel { get; set; }
        public int Index {  get; set; }
        public bool UniqueOrder { get; set; }
        public AppStatus? SelectedAppStatus { get; set; }

    }
}
