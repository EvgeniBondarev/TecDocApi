namespace OzonOrdersWeb.Areas.Studio2.ViewModels.Bitrix;

public class RemainingStockFilter
{
    public int? ProductId { get; set; }
    public string? Article { get; set; }
    public string? PreviewText { get; set; }
    public string? Active { get; set; }
    public string? Available { get; set; }
    public string? Supplier { get; set; } 
    public int? QuantityFrom { get; set; }
    public int? QuantityTo { get; set; }
    public DateTime? DateFrom { get; set; } 
    public DateTime? DateTo { get; set; }   
    
    public decimal? PriceValueFrom { get; set; }
    public decimal? PriceValueTo { get; set; }
    public decimal? RegistrationPriceFrom { get; set; }
    public decimal? RegistrationPriceTo { get; set; }
    
    public int? StoreId { get; set; }
    public string? StoreTitle { get; set; }
    public double? AmountFrom { get; set; }
    public double? AmountTo { get; set; }

    public int Page { get; set; } = 1;      
    public int PageSize { get; set; } = 50; 
    
    public string? SortColumn { get; set; }
    public string? SortDirection { get; set; } = "asc"; 
    
    public string? OzonStoreTitle { get; set; }  
    public double? OzonAmountFrom { get; set; }  
    public double? OzonAmountTo { get; set; } 
    public bool LoadOzonWarehouses { get; set; } 
}