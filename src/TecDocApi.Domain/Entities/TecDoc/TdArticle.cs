using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TecDocApi.Domain.Entities.TecDoc;

/// <summary>
/// Артикулы - основная таблица с информацией об артикулах запчастей
/// </summary>
[Table("articles")]
public class TdArticle
{
    /// <summary>
    /// Идентификатор производителя неоригинальных запчастей (связь с таблицей suppliers)
    /// </summary>
    [Column("supplierid")]
    public ushort SupplierId { get; set; }

    /// <summary>
    /// Артикул в нормальном написании (со спецсимволами)
    /// </summary>
    [Column("datasupplierarticlenumber")]
    [MaxLength(32)]
    public string DataSupplierArticleNumber { get; set; } = string.Empty;

    /// <summary>
    /// Артикул в поисковом написании (только цифры и буквы, без пробелов, дефисов, точек и т.п.)
    /// </summary>
    [Column("foundstring")]
    [MaxLength(64)]
    public string FoundString { get; set; } = string.Empty;

    /// <summary>
    /// Основное описание (наименование)
    /// </summary>
    [Column("normalizeddescription")]
    [MaxLength(128)]
    public string NormalizedDescription { get; set; } = string.Empty;

    /// <summary>
    /// Дополнительное описание (примечание)
    /// </summary>
    [Column("description")]
    [MaxLength(128)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Статус изделия (нормальный, снят с производства и др.)
    /// </summary>
    [Column("articlestatedisplayvalue")]
    [MaxLength(128)]
    public string ArticleStateDisplayValue { get; set; } = string.Empty;

    /// <summary>
    /// Упаковочная единица
    /// </summary>
    [Column("packingunit")]
    public uint? PackingUnit { get; set; }

    /// <summary>
    /// Количество в упаковке
    /// </summary>
    [Column("quantityperpackingunit")]
    public uint? QuantityPerPackingUnit { get; set; }

    /// <summary>
    /// Является сопутствующим товаром? True - если утверждение верно, False - если нет
    /// </summary>
    [Column("flagaccessory")]
    public bool FlagAccessory { get; set; }

    /// <summary>
    /// Сертифицированное сырье. True - если утверждение верно, False - если нет
    /// </summary>
    [Column("flagmaterialcertification")]
    public bool FlagMaterialCertification { get; set; }

    /// <summary>
    /// Восстановленное изделие? True - если утверждение верно, False - если нет
    /// </summary>
    [Column("flagremanufactured")]
    public bool FlagRemanufactured { get; set; }

    /// <summary>
    /// Поставляется без упаковки? True - если утверждение верно, False - если нет
    /// </summary>
    [Column("flagselfservicepacking")]
    public bool FlagSelfServicePacking { get; set; }

    /// <summary>
    /// Имеет применяемость в осях? True - если утверждение верно, False - если нет
    /// </summary>
    [Column("hasaxle")]
    public bool HasAxle { get; set; }

    /// <summary>
    /// Имеет применяемость в коммерческих ТС? True - если утверждение верно, False - если нет
    /// </summary>
    [Column("hascommercialvehicle")]
    public bool HasCommercialVehicle { get; set; }

    /// <summary>
    /// Имеет применяемость в двигателях? True - если утверждение верно, False - если нет
    /// </summary>
    [Column("hasengine")]
    public bool HasEngine { get; set; }

    /// <summary>
    /// Имеет применяемость? True - если утверждение верно, False - если нет
    /// </summary>
    [Column("haslinkitems")]
    public bool HasLinkItems { get; set; }

    /// <summary>
    /// Имеет применяемость в мототехнике? True - если утверждение верно, False - если нет
    /// </summary>
    [Column("hasmotorbike")]
    public bool HasMotorbike { get; set; }

    /// <summary>
    /// Имеет применяемость в легковых ТС? True - если утверждение верно, False - если нет
    /// </summary>
    [Column("haspassengercar")]
    public bool HasPassengerCar { get; set; }

    /// <summary>
    /// Артикул разрешен к использованию в БД? True - если утверждение верно, False - если нет
    /// </summary>
    [Column("isvalid")]
    public bool IsValid { get; set; }

    // Навигационные свойства
    public TdSupplier? Supplier { get; set; }
    public ICollection<TdArticleCross>? Crosses { get; set; }
    public ICollection<TdArticleOe>? OeNumbers { get; set; }
    public ICollection<TdArticleAttribute>? Attributes { get; set; }
    public ICollection<TdArticleImage>? Images { get; set; }
    public ICollection<TdArticleLi>? Linkages { get; set; }
    public ICollection<TdArticleEan>? EanCodes { get; set; }
    public ICollection<TdArticleInf>? Information { get; set; }
    public ICollection<TdArticleAcc>? Accessories { get; set; }
    public ICollection<TdArticleNn>? NewNumbers { get; set; }
}

