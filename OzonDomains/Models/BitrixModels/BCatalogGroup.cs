using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OzonDomains.Models.BitrixModels;

[Table("b_catalog_group")]
public class BCatalogGroup
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("NAME")]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Column("BASE")]
    [StringLength(1)]
    public string Base { get; set; } = "N";

    [Column("SORT")]
    public int Sort { get; set; } = 100;

    [Column("XML_ID")]
    [StringLength(255)]
    public string? XmlId { get; set; }

    [Column("TIMESTAMP_X")]
    public DateTime? TimestampX { get; set; }

    [Column("MODIFIED_BY")]
    public long? ModifiedBy { get; set; }

    [Column("DATE_CREATE")]
    public DateTime? DateCreate { get; set; }

    [Column("CREATED_BY")]
    public long? CreatedBy { get; set; }
    
    [NotMapped]
    public CatalogGroupType GroupType => Id switch
    {
        1 => CatalogGroupType.Retail,
        3 => CatalogGroupType.Accounting,
        4 => CatalogGroupType.Sale,
        _ => CatalogGroupType.Unknown
    };

    [NotMapped]
    public string DisplayNameRu => GroupType switch
    {
        CatalogGroupType.Retail => "Розничная цена",
        CatalogGroupType.Accounting => "Учетная цена",
        CatalogGroupType.Sale => "Sale",
        _ => Name 
    };
}