namespace TecDocApi.API.DTOs;

/// <summary>
/// Результат поиска поставщиков
/// </summary>
public class SupplierSearchResponseDto
{
    /// <summary>
    /// Количество найденных поставщиков
    /// </summary>
    /// <example>3</example>
    public int Count { get; set; }

    /// <summary>
    /// Список найденных поставщиков
    /// </summary>
    public List<SupplierDto> Results { get; set; } = new();
}

/// <summary>
/// Информация о поставщике с деталями
/// </summary>
public class SupplierDto
{
    /// <summary>
    /// Базовая информация о поставщике
    /// </summary>
    public SupplierInfoDto Supplier { get; set; } = new();

    /// <summary>
    /// Детали поставщика (адреса, контакты)
    /// </summary>
    public List<SupplierDetailDto> Details { get; set; } = new();
}

/// <summary>
/// Детали поставщика
/// </summary>
public class SupplierDetailDto
{
    /// <summary>
    /// Тип адреса
    /// </summary>
    /// <example>Main</example>
    public string AddressType { get; set; } = string.Empty;

    /// <summary>
    /// ID типа адреса
    /// </summary>
    /// <example>1</example>
    public ushort AddressTypeId { get; set; }

    /// <summary>
    /// Город 1
    /// </summary>
    /// <example>Москва</example>
    public string? City1 { get; set; }

    /// <summary>
    /// Город 2
    /// </summary>
    public string? City2 { get; set; }

    /// <summary>
    /// Код страны
    /// </summary>
    /// <example>RU</example>
    public string? CountryCode { get; set; }

    /// <summary>
    /// Email
    /// </summary>
    /// <example>info@supplier.com</example>
    public string? Email { get; set; }

    /// <summary>
    /// Факс
    /// </summary>
    /// <example>+7 (495) 123-45-67</example>
    public string? Fax { get; set; }

    /// <summary>
    /// Веб-сайт
    /// </summary>
    /// <example>https://supplier.com</example>
    public string? Homepage { get; set; }

    /// <summary>
    /// Название 1
    /// </summary>
    /// <example>ООО "Поставщик"</example>
    public string? Name1 { get; set; }

    /// <summary>
    /// Название 2
    /// </summary>
    public string? Name2 { get; set; }

    /// <summary>
    /// Почтовый индекс и город
    /// </summary>
    /// <example>123456 Москва</example>
    public string? PostalCodeCity { get; set; }

    /// <summary>
    /// Почтовый индекс абонентского ящика
    /// </summary>
    public string? PostalCodePob { get; set; }

    /// <summary>
    /// Почтовый индекс оптовика
    /// </summary>
    public string? PostalCodeWholesaler { get; set; }

    /// <summary>
    /// Почтовый код страны
    /// </summary>
    /// <example>RU</example>
    public string? PostalCountryCode { get; set; }

    /// <summary>
    /// Почтовый ящик
    /// </summary>
    public string? PostOfficeBox { get; set; }

    /// <summary>
    /// Улица 1
    /// </summary>
    /// <example>ул. Примерная, д. 1</example>
    public string? Street1 { get; set; }

    /// <summary>
    /// Улица 2
    /// </summary>
    public string? Street2 { get; set; }

    /// <summary>
    /// Телефон
    /// </summary>
    /// <example>+7 (495) 123-45-67</example>
    public string? Telephone { get; set; }
}

