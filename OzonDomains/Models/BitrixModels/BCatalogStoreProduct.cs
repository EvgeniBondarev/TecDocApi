using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OzonDomains.Models.BitrixModels;

[Table("b_catalog_store_product")]
public class BCatalogStoreProduct
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("PRODUCT_ID")]
    public int ProductId { get; set; }

    [Column("AMOUNT")]
    public double Amount { get; set; } = 0;

    [Column("STORE_ID")]
    public int StoreId { get; set; }

    [Column("QUANTITY_RESERVED")]
    public double QuantityReserved { get; set; } = 0;
}