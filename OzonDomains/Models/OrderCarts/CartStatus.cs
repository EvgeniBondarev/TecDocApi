using System.ComponentModel.DataAnnotations;

namespace OzonDomains.Models.OrderCarts;

public class CartStatus
{
    public int Id { get; set; }

    [MaxLength(100)]
    public string Name { get; set; } = null!;

    public ICollection<OrderCart> OrderCarts { get; set; } = new List<OrderCart>();
}
