using System.ComponentModel.DataAnnotations;
using OzonDomains.Models;

namespace OzonDomains;

public class Delivery
{
    public int Id { get; set; }

    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    public int ProviderId { get; set; }
    public DeliveryProvider Provider { get; set; } = null!;
        
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}