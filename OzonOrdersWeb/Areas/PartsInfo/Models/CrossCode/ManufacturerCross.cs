namespace OzonOrdersWeb.Areas.PartsInfo.Models.CrossCode;

public class CrossManufacturerModel
{
    public int Id { get; set; }
    public bool CanBeDisplayed { get; set; }
    public string Description { get; set; }
    public string FullDescription { get; set; }
    public bool HasLink { get; set; }
    public bool IsAxle { get; set; }
    public bool IsCommercialVehicle { get; set; }
    public bool IsEngine { get; set; }
    public bool IsMotorbike { get; set; }
    public bool IsPassengerCar { get; set; }
    public bool IsTransporter { get; set; }
    public bool IsVgl { get; set; }
    public string MatchCode { get; set; }
}