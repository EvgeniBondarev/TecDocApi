using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TecDocApi.Domain.Entities.TecDoc;

/// <summary>
/// Информация/Описание - таблица с дополнительной информацией и описаниями для артикулов
/// </summary>
[Table("article_inf")]
public class TdArticleInf
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
    /// Идентификатор типа информации
    /// </summary>
    [Column("informationtypekey")]
    public ushort InformationTypeKey { get; set; }

    /// <summary>
    /// Текст дополнительной информации
    /// </summary>
    [Column("informationtext", TypeName = "text")]
    public string InformationText { get; set; } = string.Empty;

    /// <summary>
    /// Тип информации
    /// </summary>
    [Column("informationtype")]
    [MaxLength(64)]
    public string InformationType { get; set; } = string.Empty;

    // Навигационные свойства
    public TdArticle? Article { get; set; }
    public TdSupplier? Supplier { get; set; }
}

