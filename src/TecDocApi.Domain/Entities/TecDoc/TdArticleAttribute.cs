using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TecDocApi.Domain.Entities.TecDoc;

/// <summary>
/// Характеристики/Критерии - таблица с характеристиками и критериями артикулов
/// </summary>
[Table("article_attributes")]
public class TdArticleAttribute
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
    /// Идентификатор критерия
    /// </summary>
    [Column("id")]
    public ushort Id { get; set; }

    /// <summary>
    /// Описание критерия (ключ)
    /// </summary>
    [Column("description")]
    [MaxLength(128)]
    public string? Description { get; set; }

    /// <summary>
    /// Уточнение критерия (дополнение)
    /// </summary>
    [Column("displaytitle")]
    [MaxLength(128)]
    public string DisplayTitle { get; set; } = string.Empty;

    /// <summary>
    /// Свойство критерия (значение)
    /// </summary>
    [Column("displayvalue")]
    [MaxLength(4000)]
    public string DisplayValue { get; set; } = string.Empty;

    // Навигационные свойства
    public TdArticle? Article { get; set; }
    public TdSupplier? Supplier { get; set; }
}

