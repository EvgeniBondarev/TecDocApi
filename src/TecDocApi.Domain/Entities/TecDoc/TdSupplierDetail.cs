using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TecDocApi.Domain.Entities.TecDoc;

/// <summary>
/// Детали поставщиков - таблица с адресной информацией и контактами поставщиков
/// </summary>
[Table("supplier_details")]
public class TdSupplierDetail
{
    /// <summary>
    /// Идентификатор поставщика (связь с таблицей suppliers)
    /// </summary>
    [Column("supplierid")]
    public ushort SupplierId { get; set; }

    /// <summary>
    /// Идентификатор типа адреса
    /// </summary>
    [Column("addresstypeid")]
    [MaxLength(1)]
    public string AddressTypeId { get; set; } = string.Empty;

    /// <summary>
    /// Тип адреса
    /// </summary>
    [Column("addresstype")]
    [MaxLength(32)]
    public string? AddressType { get; set; }

    /// <summary>
    /// Название организации (основное)
    /// </summary>
    [Column("name1")]
    [MaxLength(64)]
    public string? Name1 { get; set; }

    /// <summary>
    /// Название организации (дополнительное)
    /// </summary>
    [Column("name2")]
    [MaxLength(64)]
    public string? Name2 { get; set; }

    /// <summary>
    /// Улица (основная)
    /// </summary>
    [Column("street1")]
    [MaxLength(64)]
    public string? Street1 { get; set; }

    /// <summary>
    /// Улица (дополнительная)
    /// </summary>
    [Column("street2")]
    [MaxLength(64)]
    public string? Street2 { get; set; }

    /// <summary>
    /// Город (основной)
    /// </summary>
    [Column("city1")]
    [MaxLength(64)]
    public string? City1 { get; set; }

    /// <summary>
    /// Город (дополнительный)
    /// </summary>
    [Column("city2")]
    [MaxLength(64)]
    public string? City2 { get; set; }

    /// <summary>
    /// Почтовый индекс города
    /// </summary>
    [Column("postalcodecity")]
    [MaxLength(32)]
    public string? PostalCodeCity { get; set; }

    /// <summary>
    /// Телефон
    /// </summary>
    [Column("telephone")]
    [MaxLength(32)]
    public string? Telephone { get; set; }

    /// <summary>
    /// Факс
    /// </summary>
    [Column("fax")]
    [MaxLength(64)]
    public string? Fax { get; set; }

    /// <summary>
    /// Email
    /// </summary>
    [Column("email")]
    [MaxLength(64)]
    public string? Email { get; set; }

    /// <summary>
    /// Домашняя страница
    /// </summary>
    [Column("homepage")]
    [MaxLength(64)]
    public string? Homepage { get; set; }

    /// <summary>
    /// Почтовый индекс почтового ящика
    /// </summary>
    [Column("postalcodepob")]
    [MaxLength(32)]
    public string? PostalCodePob { get; set; }

    /// <summary>
    /// Почтовый индекс оптовика
    /// </summary>
    [Column("postalcodewholesaler")]
    [MaxLength(32)]
    public string? PostalCodeWholesaler { get; set; }

    /// <summary>
    /// Почтовый код страны
    /// </summary>
    [Column("postalcountrycode")]
    [MaxLength(32)]
    public string? PostalCountryCode { get; set; }

    /// <summary>
    /// Почтовый ящик
    /// </summary>
    [Column("postofficebox")]
    [MaxLength(32)]
    public string? PostOfficeBox { get; set; }

    /// <summary>
    /// Код страны
    /// </summary>
    [Column("countrycode")]
    [MaxLength(64)]
    public string? CountryCode { get; set; }

    // Навигационные свойства
    public TdSupplier? Supplier { get; set; }
}

