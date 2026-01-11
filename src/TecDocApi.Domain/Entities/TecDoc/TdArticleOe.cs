using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TecDocApi.Domain.Entities.TecDoc;

/// <summary>
/// Оригинальные кросс-номера - таблица с OEM номерами для артикулов
/// </summary>
[Table("article_oe")]
public class TdArticleOe
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
    /// Оригинальный номер
    /// </summary>
    [Column("oenbr")]
    [MaxLength(64)]
    public string OENbr { get; set; } = string.Empty;

    /// <summary>
    /// Является аддитивным? True - если утверждение верно, False - если нет
    /// </summary>
    [Column("isadditive")]
    public bool IsAdditive { get; set; }

    // Навигационные свойства
    public TdArticle? Article { get; set; }
    public TdSupplier? Supplier { get; set; }
}

