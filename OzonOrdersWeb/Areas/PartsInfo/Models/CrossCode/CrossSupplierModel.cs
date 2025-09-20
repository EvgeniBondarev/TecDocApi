namespace OzonOrdersWeb.Areas.PartsInfo.Models.CrossCode;

public class CrossSupplierModel
{
    public int Id { get; set; }
    public int DataVersion { get; set; }
    public string Description { get; set; }
    public string MatchCode { get; set; }
    public long NbrOfArticles { get; set; }
    public bool HasNewVersionArticles { get; set; }
    public string Img { get; set; }
}
