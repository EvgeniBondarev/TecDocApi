using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TecDocApi.Domain.Entities.TecDoc;

/// <summary>
/// Атрибуты легковых автомобилей - таблица с характеристиками и атрибутами легковых автомобилей
/// </summary>
[Table("passanger_car_attributes")]
public class TdPassengerCarAttribute
{
    /// <summary>
    /// Идентификатор легкового автомобиля (связь с таблицей passanger_cars)
    /// </summary>
    [Column("passangercarid")]
    public uint PassengerCarId { get; set; }

    /// <summary>
    /// Группа атрибута
    /// </summary>
    [Column("attributegroup")]
    [MaxLength(64)]
    public string? AttributeGroup { get; set; }

    /// <summary>
    /// Тип атрибута
    /// </summary>
    [Column("attributetype")]
    [MaxLength(64)]
    public string? AttributeType { get; set; }

    /// <summary>
    /// Заголовок для отображения
    /// </summary>
    [Column("displaytitle")]
    [MaxLength(255)]
    public string? DisplayTitle { get; set; }

    /// <summary>
    /// Значение для отображения
    /// </summary>
    [Column("displayvalue")]
    [MaxLength(255)]
    public string? DisplayValue { get; set; }

    // Навигационные свойства
    public TdPassengerCar? PassengerCar { get; set; }
}

