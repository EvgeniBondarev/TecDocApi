namespace OzonDomains.Models.BitrixModels;

public class BitrixModel
{
    public decimal Price { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Group { get; set; }
    public Dictionary<string, double> Stores { get; set; } = new Dictionary<string, double>();
}
