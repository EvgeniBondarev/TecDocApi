namespace OzonOrdersWeb.Areas.PartsInfo.Models.ABCP;

public class ArticleMetaModel
{
    public int ProductId { get; set; }
    public int Wearout { get; set; }
    public bool IsUsed { get; set; }
    public string[]? Images { get; set; }
    public string AbcpWh { get; set; } = string.Empty;
}