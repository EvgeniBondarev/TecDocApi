using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TecDocApi.Domain.Entities.TecDoc;

/// <summary>
/// Поставщики - таблица с информацией о производителях неоригинальных запчастей
/// </summary>
[Table("suppliers")]
public class TdSupplier
{
    /// <summary>
    /// Идентификатор поставщика
    /// </summary>
    [Key]
    [Column("id")]
    public ushort Id { get; set; }

    /// <summary>
    /// Описание поставщика
    /// </summary>
    [Column("description")]
    [MaxLength(32)]
    public string? Description { get; set; }

    /// <summary>
    /// Matchcode поставщика (код для поиска)
    /// </summary>
    [Column("matchcode")]
    [MaxLength(32)]
    public string? Matchcode { get; set; }

    /// <summary>
    /// Версия данных поставщика
    /// </summary>
    [Column("dataversion")]
    public ushort? DataVersion { get; set; }

    /// <summary>
    /// Количество артикулов у поставщика
    /// </summary>
    [Column("nbrofarticles")]
    public uint? NbrOfArticles { get; set; }

    /// <summary>
    /// Имеет артикулы новой версии? True - если утверждение верно, False - если нет
    /// </summary>
    [Column("hasnewversionarticles")]
    public bool? HasNewVersionArticles { get; set; }

    // Навигационные свойства
    public ICollection<TdArticle>? Articles { get; set; }
    public ICollection<TdSupplierDetail>? Details { get; set; }
}

