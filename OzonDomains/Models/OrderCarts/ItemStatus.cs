using System.ComponentModel.DataAnnotations;

namespace OzonDomains.Models.OrderCarts;

public class ItemStatus
{
    public int Id { get; set; }

    [MaxLength(100)]
    public string Name { get; set; } = null!;

    public int? StatusColorId { get; set; }  
    public StatusColor? StatusColor { get; set; } 

    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
