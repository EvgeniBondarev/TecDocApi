using System.ComponentModel.DataAnnotations;

namespace OzonDomains.Models.OrderCarts;

public class OrderCart
{
    public int Id { get; set; }

    [MaxLength(100)]
    public string Provider { get; set; } = null!;

    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    [MaxLength(250)]
    public string? Comment { get; set; }

    [MaxLength(100)]
    public string? ClientOrderNumber { get; set; }

    public int? CartStatusId { get; set; }
    public CartStatus? CartStatus { get; set; }
    
    [Display(Name = "Дата создания")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
