using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OzonDomains.Models.BitrixModels;

[Table("b_catalog_price")]
public class BCatalogPrice
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("PRODUCT_ID")]
    public int ProductId { get; set; }

    [Column("EXTRA_ID")]
    public int? ExtraId { get; set; }

    [Column("CATALOG_GROUP_ID")]
    public int CatalogGroupId { get; set; }

    [Column("PRICE", TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    [Column("CURRENCY")]
    [StringLength(3)]
    public string Currency { get; set; } = string.Empty;

    [Column("TIMESTAMP_X")]
    public DateTime TimestampX { get; set; }

    [Column("QUANTITY_FROM")]
    public int? QuantityFrom { get; set; }

    [Column("QUANTITY_TO")]
    public int? QuantityTo { get; set; }

    [Column("TMP_ID")]
    [StringLength(40)]
    public string? TmpId { get; set; }

    [Column("PRICE_SCALE", TypeName = "decimal(26,12)")]
    public decimal? PriceScale { get; set; }
}