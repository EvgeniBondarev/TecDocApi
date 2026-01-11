using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TecDocApi.Domain.Entities.TecDoc;

/// <summary>
/// Сопутствующие товары/Аксессуары - таблица со связями артикулов с сопутствующими товарами и аксессуарами
/// </summary>
[Table("article_acc")]
public class TdArticleAcc
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
    /// Аксессуар: идентификатор бренда (связь с таблицей suppliers)
    /// </summary>
    [Column("accsupplierid")]
    public ushort AccSupplierId { get; set; }

    /// <summary>
    /// Аксессуар: артикул (связь с таблицей articles)
    /// </summary>
    [Column("accdatasupplierarticlenumber")]
    [MaxLength(32)]
    public string AccDataSupplierArticleNumber { get; set; } = string.Empty;

    // Навигационные свойства
    public TdArticle? Article { get; set; }
    public TdSupplier? Supplier { get; set; }
    public TdSupplier? AccSupplier { get; set; }
}

