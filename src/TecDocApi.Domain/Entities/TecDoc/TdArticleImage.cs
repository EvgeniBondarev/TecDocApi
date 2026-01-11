using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TecDocApi.Domain.Entities.TecDoc;

/// <summary>
/// Изображения и файлы - таблица с изображениями и документами для артикулов
/// </summary>
[Table("article_images")]
public class TdArticleImage
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
    /// Имя файла
    /// </summary>
    [Column("picturename")]
    [MaxLength(64)]
    public string PictureName { get; set; } = string.Empty;

    /// <summary>
    /// Тип документа
    /// </summary>
    [Column("description")]
    [MaxLength(64)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Описание документа
    /// </summary>
    [Column("additionaldescription")]
    [MaxLength(64)]
    public string AdditionalDescription { get; set; } = string.Empty;

    /// <summary>
    /// Название документа
    /// </summary>
    [Column("documentname")]
    [MaxLength(128)]
    public string DocumentName { get; set; } = string.Empty;

    /// <summary>
    /// Формат документа
    /// </summary>
    [Column("documenttype")]
    [MaxLength(8)]
    public string DocumentType { get; set; } = string.Empty;

    /// <summary>
    /// Идентификатор типа документа
    /// </summary>
    [Column("normeddescriptionid")]
    public ushort NormedDescriptionId { get; set; }

    /// <summary>
    /// Использовать в качестве превью? True - если утверждение верно, False - если нет
    /// </summary>
    [Column("showimmediately")]
    public bool ShowImmediately { get; set; }

    // Навигационные свойства
    public TdArticle? Article { get; set; }
    public TdSupplier? Supplier { get; set; }
}

