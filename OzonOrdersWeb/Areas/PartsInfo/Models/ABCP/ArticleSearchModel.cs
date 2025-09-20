namespace OzonOrdersWeb.Areas.PartsInfo.Models.ABCP;

public class ArticleSearchModel
{
    public int DistributorId { get; set; }
    public string? Grp { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Number { get; set; } = string.Empty;
    public string NumberFix { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Availability { get; set; }
    public int Packing { get; set; }
    public int DeliveryPeriod { get; set; }
    public int DeliveryPeriodMax { get; set; }
    public string DeadlineReplace { get; set; } = string.Empty;
    public string DistributorCode { get; set; } = string.Empty;
    public int SupplierCode { get; set; }
    public string? SupplierColor { get; set; }
    public string SupplierDescription { get; set; } = string.Empty;
    public string ItemKey { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal MaxPrice { get; set; }
    public decimal Weight { get; set; }
    public decimal? Volume { get; set; }
    public DateTime LastUpdateTime { get; set; }
    public decimal AdditionalPrice { get; set; }
    public bool NoReturn { get; set; }
    public bool IsUsed { get; set; }
    public ArticleMetaModel Meta { get; set; } = new();
    public int DeliveryProbability { get; set; }
    public string DescriptionOfDeliveryProbability { get; set; } = string.Empty;
    public decimal PriceIn { get; set; }
    public decimal PriceRate { get; set; }
    public bool IsAnalog { get; set; }
}