namespace OzonOrdersWeb.Areas.PartsInfo.Models.Substitute;

public class VehicleModel
{
    public string ModelName { get; set; }
    public int ModelId { get; set; }
    public List<Substitute> Substitutes { get; set; }
}
