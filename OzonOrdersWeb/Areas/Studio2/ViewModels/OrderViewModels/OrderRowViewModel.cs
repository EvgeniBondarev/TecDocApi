using OzonDomains.Models;

namespace OzonOrdersWeb.ViewModels.OrderViewModels
{
    public class OrderRowViewModel
    {
        public Order Order { get; set; }
        public int Index { get; set; }
        public List<string> UnavailableStatuses { get; set; } = new List<string>
        {
            "Доставлен",
            "Отменён",
            "Отменен",
            "Доставляется",
            "Ожидает в ПВЗ",
            "Отменен при до.",
            "Отменен при об",
            "Отгружен прода",
            "Отменен продав",
            "Отгружен постащиком",
            "Отгружен поста"
        };
        public bool OrderWithOneMatches { get; set; }
        public Manufacturer FileOrderManufacturer { get; set; }
        public int OrderId { get; set; }

        public bool CheckedCase1
        {
            get
            {
                return Order != null && Order.AppStatus != null && 
                    Order.AppStatus.Name == "Заказан поставщику" && Order.Status == "Доставлен";
            }
        }

        public bool CheckedCase2
        {
            get
            {
                return Order != null && Order.AppStatus != null && 
                    Order.AppStatus.Name == "Заказан поставщику" && Order.Status == "Доставляется";
            }
        }

        public bool CheckedCase3
        {
            get
            {
                return Order != null && Order.AppStatus != null &&
                    Order.AppStatus.Name == "Отгружен поставщиком";
            }
        }

        public bool CheckedCase4
        {
            get
            {
                return Order != null && Order.Status == "Отменён";
            }
        }
        
        public bool CheckedCase5
        {
            get
            {
                return Order != null && Order.AppStatus != null &&
                       Order.AppStatus.Name == "Отменен";
            }
        }
    }
}
