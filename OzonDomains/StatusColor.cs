using System.ComponentModel.DataAnnotations;
using OzonDomains.Models.OrderCarts;

namespace OzonDomains;

public class StatusColor
{
    public int Id { get; set; }

    [MaxLength(20)]
    public string ColorCode { get; set; } = null!;
}
