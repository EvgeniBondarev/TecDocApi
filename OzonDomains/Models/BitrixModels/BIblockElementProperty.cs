using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

[Table("b_iblock_element_property")]
public class BIblockElementProperty
{
    [Key]
    [Column("ID")]
    public long Id { get; set; }

    [Column("IBLOCK_PROPERTY_ID")]
    public int IblockPropertyId { get; set; }

    [Column("IBLOCK_ELEMENT_ID")]
    public int IblockElementId { get; set; }

    [Column("VALUE", TypeName = "mediumtext")]
    public string Value { get; set; } = string.Empty;

    [Column("VALUE_TYPE")]
    [StringLength(4)]
    public string ValueType { get; set; } = "text";

    [Column("VALUE_ENUM")]
    public int? ValueEnum { get; set; }

    [Column("VALUE_NUM", TypeName = "decimal(18,4)")]
    public decimal? ValueNum { get; set; }

    [Column("DESCRIPTION")]
    [StringLength(255)]
    public string? Description { get; set; }
}