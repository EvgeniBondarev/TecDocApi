namespace OzonOrdersWeb.Areas.PartsInfo.Models.ABCP;

public class TipsResponseModel
{
    public IEnumerable<TipModel> Tips { get; set; }
    public string OriginalNumber { get; set; }
}