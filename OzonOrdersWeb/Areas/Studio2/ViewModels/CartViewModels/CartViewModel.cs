using OzonDomains.Models;

namespace OzonOrdersWeb.Areas.Studio2.ViewModels.CartViewModels;

public class CartViewModel
{
    public Order Order { get; set; }
    public int Id { get; set; }
    public string ItemId { get; set; }
    public int Quantity { get; set; }
    public string OrderItemCode { get; set; }
    public string ItemStatus { get; set; }
}