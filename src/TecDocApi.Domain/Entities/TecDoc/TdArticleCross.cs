using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TecDocApi.Domain.Entities.TecDoc;

/// <summary>
/// Кросс-номера - таблица с оригинальными номерами производителей ТС для артикулов
/// </summary>
[Table("article_cross")]
public class TdArticleCross
{
    /// <summary>
    /// Идентификатор производителя ТС (связь с таблицей manufacturers)
    /// </summary>
    [Column("manufacturerid")]
    public uint ManufacturerId { get; set; }

    /// <summary>
    /// Оригинальный номер
    /// </summary>
    [Column("oenbr")]
    [MaxLength(64)]
    public string OENbr { get; set; } = string.Empty;

    /// <summary>
    /// Идентификатор производителя неоригинальных запчастей (связь с таблицей suppliers)
    /// </summary>
    [Column("supplierid")]
    public ushort SupplierId { get; set; }

    /// <summary>
    /// Артикул (связь с таблицей articles)
    /// </summary>
    [Column("partsdatasupplierarticlenumber")]
    [MaxLength(32)]
    public string PartsDataSupplierArticleNumber { get; set; } = string.Empty;

    // Навигационные свойства
    public TdArticle? Article { get; set; }
    public TdSupplier? Supplier { get; set; }
    public TdManufacturer? Manufacturer { get; set; }
}

