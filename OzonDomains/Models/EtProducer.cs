using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OzonDomains.Models;

[Table("et_producers")]
public class EtProducer
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("realid")]
    public int RealId { get; set; }

    [Column("prefix")]
    [MaxLength(20)]
    public string Prefix { get; set; } = string.Empty;

    [Column("name")]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Column("address")]
    [MaxLength(100)]
    public string Address { get; set; } = string.Empty;

    [Column("www")]
    [MaxLength(100)]
    public string Www { get; set; } = string.Empty;

    [Column("rating")]
    public int Rating { get; set; }

    [Column("existName")]
    [MaxLength(100)]
    public string ExistName { get; set; } = string.Empty;

    [Column("existId")]
    public int ExistId { get; set; }

    [Column("domain")]
    [MaxLength(100)]
    public string Domain { get; set; } = string.Empty;

    [Column("tecdocSupplierId")]
    public int TecdocSupplierId { get; set; }

    [Column("marketPrefix")]
    [MaxLength(3)]
    public string MarketPrefix { get; set; } = string.Empty;
}