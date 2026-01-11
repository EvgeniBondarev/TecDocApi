using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TecDocApi.Domain.Entities.TecDoc;

/// <summary>
/// Новые номера - таблица с информацией о новых номерах артикулов (замена старых номеров)
/// </summary>
[Table("article_nn")]
public class TdArticleNn
{
    /// <summary>
    /// Идентификатор производителя неоригинальных запчастей (связь с таблицей suppliers)
    /// </summary>
    [Column("supplierid")]
    public ushort SupplierId { get; set; }

    /// <summary>
    /// Артикул (связь с таблицей articles)
    /// </summary>
    [Column("datasupplierarticlenumber")]
    [MaxLength(32)]
    public string DataSupplierArticleNumber { get; set; } = string.Empty;

    /// <summary>
    /// Новый идентификатор бренда (связь с таблицей suppliers)
    /// </summary>
    [Column("newsupplierid")]
    public ushort NewSupplierId { get; set; }

    /// <summary>
    /// Новый артикул (связь с таблицей articles)
    /// </summary>
    [Column("newdatasupplierarticlenumber")]
    [MaxLength(32)]
    public string NewDataSupplierArticleNumber { get; set; } = string.Empty;

    /// <summary>
    /// Новый артикул (дубль newdatasupplierarticlenumber)
    /// Примечание: Колонка может отсутствовать в некоторых версиях БД
    /// </summary>
    [Column("newbr")]
    [MaxLength(32)]
    public string? NewNbr { get; set; }

    // Навигационные свойства
    public TdArticle? Article { get; set; }
    public TdSupplier? Supplier { get; set; }
    public TdSupplier? NewSupplier { get; set; }
}

