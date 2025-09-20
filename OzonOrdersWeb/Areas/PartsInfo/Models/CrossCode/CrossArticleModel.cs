namespace OzonOrdersWeb.Areas.PartsInfo.Models.CrossCode;

public class CrossArticleModel
{
    public int SupplierId { get; set; }
    public string DataSupplierArticleNumber { get; set; }
    public bool IsAdditive { get; set; }
    public string OENbr { get; set; }
    public int ManufacturerId { get; set; }
    public CrossSupplierModel Supplier { get; set; }
    public CrossManufacturerModel Manufacturer { get; set; }
}