namespace OzonOrdersWeb.Areas.PartsInfo.Models.Substitute;

public class Substitute
{
    public string Type { get; set; }
    public string Name { get; set; }
    public int ModelId { get; set; }
    public Modification Modification { get; set; }
    public List<SubAttribute> Attributes { get; set; }
}