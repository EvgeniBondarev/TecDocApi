using OzonDomains.Models;

namespace OzonOrdersWeb.Areas.Studio2.ViewModels.ShippedBySupplierTransaction;

public class ShippedBySupplierTransactionViewModel
{
    public List<Order> Orders { get; set; }
    public string UserName { get; set; }
    public string Comment { get; set; }
    public int Page { get; set; }
    public string DeletedOrders { get; set; }
    public bool ProcessIn1C { get; set; }
    
    public string NDS { get; set; }
    public string Contract { get; set; }
    public string NumberVh { get; set; }
    public DateTime DateVh { get; set; }
    public string? TransactionComment { get; set; }
}