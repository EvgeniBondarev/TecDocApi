using System.ComponentModel.DataAnnotations;

namespace OzonDomains.Models.OrderCarts;

public class OrderItem
{
    public int Id { get; set; }

    [MaxLength(100)]
    public string ItemId { get; set; } = null!;
    public int Quantity { get; set; }

    public int? StudioOrderId { get; set; } 
    public Order? StudioOrder { get; set; } 

    [MaxLength(250)]
    public string? Comment { get; set; }

    public int? ItemStatusId { get; set; }
    public ItemStatus? ItemStatus { get; set; }

    public int OrderCartId { get; set; }
    public OrderCarts.OrderCart OrderCart { get; set; } = null!;
    
    [MaxLength(100)] 
    public string? OrderItemCode { get; set; }
}
