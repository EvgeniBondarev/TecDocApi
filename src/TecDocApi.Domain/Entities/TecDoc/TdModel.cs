using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TecDocApi.Domain.Entities.TecDoc;

/// <summary>
/// Модели - таблица с информацией о моделях транспортных средств
/// </summary>
[Table("models")]
public class TdModel
{
    /// <summary>
    /// Идентификатор модели
    /// </summary>
    [Key]
    [Column("id")]
    public uint Id { get; set; }

    /// <summary>
    /// Описание модели
    /// </summary>
    [Column("description")]
    [MaxLength(128)]
    public string? Description { get; set; }

    /// <summary>
    /// Полное описание модели
    /// </summary>
    [Column("fulldescription")]
    [MaxLength(128)]
    public string? FullDescription { get; set; }

    /// <summary>
    /// Интервал производства
    /// </summary>
    [Column("constructioninterval")]
    [MaxLength(24)]
    public string? ConstructionInterval { get; set; }

    /// <summary>
    /// Идентификатор производителя (связь с таблицей manufacturers)
    /// </summary>
    [Column("manufacturerid")]
    public uint? ManufacturerId { get; set; }

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
    public TdManufacturer? Manufacturer { get; set; }
    public ICollection<TdPassengerCar>? PassengerCars { get; set; }
}

