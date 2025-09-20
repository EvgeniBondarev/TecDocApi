using System.ComponentModel.DataAnnotations.Schema;

namespace OzonDomains.Models;

[Table("WarehouseMapping")]
public class WarehouseMapping
{
    public int Id { get; set; }
    public string BitrixWarehouseName { get; set; }
    public string OzonWarehouseName { get; set; }
    public int? OzonClientId { get; set; }

    [ForeignKey("OzonClientId")]
    public virtual OzonClient? OzonClient { get; set; } 
}
