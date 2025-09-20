using OzonDomains.Models;

public class OrderHistoryViewModel
{
    public int OrderId { get; set; }
    public string? ShipmentNumber { get; set; }
    public string? ProductName { get; set; }
    public string? Article { get; set; }
    public string? ClientName { get; set; }

    public Dictionary<DateTime, List<OrderHistory>> Histories { get; set; }
}