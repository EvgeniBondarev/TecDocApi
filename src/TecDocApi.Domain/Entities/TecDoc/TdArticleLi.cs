using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TecDocApi.Domain.Entities.TecDoc;

/// <summary>
/// Применяемость - таблица с информацией о применяемости артикулов к транспортным средствам и узлам
/// </summary>
[Table("article_li")]
public class TdArticleLi
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
    /// Тип группы применяемости (Axle, CommercialVehicle, CVManufacturer, Engine, Motorbike, PassengerCar)
    /// </summary>
    [Column("linkagetypeid")]
    [MaxLength(32)]
    public string LinkageTypeId { get; set; } = string.Empty;

    /// <summary>
    /// Идентификатор ТС или узла
    /// </summary>
    [Column("linkageid")]
    public uint LinkageId { get; set; }

    // Навигационные свойства
    public TdArticle? Article { get; set; }
    public TdSupplier? Supplier { get; set; }
}

