using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TecDocApi.Domain.Entities.TecDoc;

/// <summary>
/// Производители - таблица с информацией о производителях транспортных средств
/// </summary>
[Table("manufacturers")]
public class TdManufacturer
{
    /// <summary>
    /// Идентификатор производителя
    /// </summary>
    [Key]
    [Column("id")]
    public uint Id { get; set; }

    /// <summary>
    /// Описание производителя
    /// </summary>
    [Column("description")]
    [MaxLength(255)]
    public string? Description { get; set; }

    // Навигационные свойства
    public ICollection<TdArticleCross>? ArticleCrosses { get; set; }
}

