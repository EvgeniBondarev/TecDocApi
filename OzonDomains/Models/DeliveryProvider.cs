using System.ComponentModel.DataAnnotations;

namespace OzonDomains.Models;

public class DeliveryProvider
{
    public int Id { get; set; }

    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    public ICollection<Delivery> Deliveries { get; set; } = new List<Delivery>();
}