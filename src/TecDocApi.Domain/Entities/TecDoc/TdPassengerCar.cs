using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TecDocApi.Domain.Entities.TecDoc;

/// <summary>
/// Легковые автомобили - таблица с информацией о легковых транспортных средствах
/// </summary>
[Table("passanger_cars")]
public class TdPassengerCar
{
    /// <summary>
    /// Идентификатор легкового автомобиля
    /// </summary>
    [Key]
    [Column("id")]
    public uint Id { get; set; }

    /// <summary>
    /// Описание легкового автомобиля
    /// </summary>
    [Column("description")]
    [MaxLength(255)]
    public string? Description { get; set; }

    /// <summary>
    /// Полное описание легкового автомобиля
    /// </summary>
    [Column("fulldescription")]
    [MaxLength(255)]
    public string? FullDescription { get; set; }

    /// <summary>
    /// Интервал производства
    /// </summary>
    [Column("constructioninterval")]
    [MaxLength(64)]
    public string? ConstructionInterval { get; set; }

    /// <summary>
    /// Идентификатор модели (связь с таблицей models)
    /// </summary>
    [Column("modelid")]
    public uint? ModelId { get; set; }

    /// <summary>
    /// Может быть отображен? True - если утверждение верно, False - если нет
    /// </summary>
    [Column("canbedisplayed")]
    public bool? CanBeDisplayed { get; set; }

    /// <summary>
    /// Имеет связь? True - если утверждение верно, False - если нет
    /// </summary>
    [Column("haslink")]
    public bool? HasLink { get; set; }

    /// <summary>
    /// Является осью? True - если утверждение верно, False - если нет
    /// </summary>
    [Column("isaxle")]
    public bool IsAxle { get; set; }

    /// <summary>
    /// Является коммерческим транспортным средством? True - если утверждение верно, False - если нет
    /// </summary>
    [Column("iscommercialvehicle")]
    public bool IsCommercialVehicle { get; set; }

    /// <summary>
    /// Является идентификатором производителя коммерческих ТС? True - если утверждение верно, False - если нет
    /// </summary>
    [Column("iscvmanufacturerid")]
    public bool IsCvManufacturerId { get; set; }

    /// <summary>
    /// Является двигателем? True - если утверждение верно, False - если нет
    /// </summary>
    [Column("isengine")]
    public bool IsEngine { get; set; }

    /// <summary>
    /// Является мотоциклом? True - если утверждение верно, False - если нет
    /// </summary>
    [Column("ismotorbike")]
    public bool IsMotorbike { get; set; }

    /// <summary>
    /// Является легковым автомобилем? True - если утверждение верно, False - если нет
    /// </summary>
    [Column("ispassengercar")]
    public bool IsPassengerCar { get; set; }

    /// <summary>
    /// Является транспортером? True - если утверждение верно, False - если нет
    /// </summary>
    [Column("istransporter")]
    public bool IsTransporter { get; set; }

    // Навигационные свойства
    public TdModel? Model { get; set; }
    public ICollection<TdPassengerCarAttribute>? Attributes { get; set; }
}

