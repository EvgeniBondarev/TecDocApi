using OzonDomains.Models;
using OzonDomains.Models.BitrixModels;

namespace OzonOrdersWeb.Areas.Studio2.ViewModels.Bitrix;

public class RemainingStockViewModel
{
    public PagedResult<RemainingStockBitrix> Result { get; set; }
    public RemainingStockFilter Filter { get; set; }
    public List<Supplier> Suppliers { get; set; }
    public List<Supplier> AllSuppliers { get; set; }
    public List<WarehouseMapping> WarehousesmMappings { get; set; }
    public List<string> ActiveStores { get; set; } = new();
    public decimal RateUSD { get; set; }
    public decimal RateEUR { get; set;}
    public decimal RateBYN { get; set; }
}