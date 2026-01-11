namespace TecDocApi.API.DTOs;

/// <summary>
/// Результат поиска артикулов
/// </summary>
public class ArticleSearchResponseDto
{
    /// <summary>
    /// Количество найденных артикулов
    /// </summary>
    /// <example>5</example>
    public int Count { get; set; }

    /// <summary>
    /// Список найденных артикулов
    /// </summary>
    public List<ArticleDto> Results { get; set; } = new();
}

/// <summary>
/// Информация об артикуле
/// </summary>
public class ArticleDto
{
    /// <summary>
    /// Базовая информация об артикуле
    /// </summary>
    public ArticleInfoDto Article { get; set; } = new();

    /// <summary>
    /// Информация о поставщике
    /// </summary>
    public SupplierInfoDto? Supplier { get; set; }

    /// <summary>
    /// Кроссы (аналоги) артикула
    /// </summary>
    public List<CrossDto> Crosses { get; set; } = new();

    /// <summary>
    /// OEM номера
    /// </summary>
    public List<OeNumberDto> OeNumbers { get; set; } = new();

    /// <summary>
    /// Атрибуты артикула
    /// </summary>
    public List<AttributeDto> Attributes { get; set; } = new();

    /// <summary>
    /// Изображения артикула
    /// </summary>
    public List<ImageDto> Images { get; set; } = new();

    /// <summary>
    /// Применимость (linkages)
    /// </summary>
    public List<LinkageDto> Linkages { get; set; } = new();

    /// <summary>
    /// EAN коды
    /// </summary>
    public List<EanCodeDto> EanCodes { get; set; } = new();

    /// <summary>
    /// Дополнительная информация
    /// </summary>
    public List<InformationDto> Information { get; set; } = new();

    /// <summary>
    /// Аксессуары
    /// </summary>
    public List<AccessoryDto> Accessories { get; set; } = new();

    /// <summary>
    /// Новые номера
    /// </summary>
    public List<NewNumberDto> NewNumbers { get; set; } = new();
}

/// <summary>
/// Базовая информация об артикуле
/// </summary>
public class ArticleInfoDto
{
    /// <summary>
    /// ID поставщика
    /// </summary>
    /// <example>7</example>
    public ushort SupplierId { get; set; }

    /// <summary>
    /// Номер артикула поставщика
    /// </summary>
    /// <example>ABC-123</example>
    public string DataSupplierArticleNumber { get; set; } = string.Empty;

    /// <summary>
    /// Нормализованная строка поиска
    /// </summary>
    /// <example>ABC123</example>
    public string FoundString { get; set; } = string.Empty;

    /// <summary>
    /// Нормализованное описание артикула
    /// </summary>
    /// <example>Глушитель</example>
    public string? NormalizedDescription { get; set; }

    /// <summary>
    /// Описание артикула
    /// </summary>
    /// <example>Тормозная колодка передняя</example>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Состояние артикула
    /// </summary>
    /// <example>Valid</example>
    public string ArticleStateDisplayValue { get; set; } = string.Empty;

    /// <summary>
    /// Количество в упаковке
    /// </summary>
    /// <example>2</example>
    public uint? QuantityPerPackingUnit { get; set; }

    /// <summary>
    /// Флаги артикула
    /// </summary>
    public ArticleFlagsDto Flags { get; set; } = new();
}

/// <summary>
/// Флаги артикула
/// </summary>
public class ArticleFlagsDto
{
    /// <summary>
    /// Является аксессуаром
    /// </summary>
    /// <example>false</example>
    public bool FlagAccessory { get; set; }

    /// <summary>
    /// Имеет сертификацию материалов
    /// </summary>
    /// <example>true</example>
    public bool FlagMaterialCertification { get; set; }

    /// <summary>
    /// Восстановленный
    /// </summary>
    /// <example>false</example>
    public bool FlagRemanufactured { get; set; }

    /// <summary>
    /// Упаковка для самообслуживания
    /// </summary>
    /// <example>false</example>
    public bool FlagSelfServicePacking { get; set; }

    /// <summary>
    /// Применим к осям
    /// </summary>
    /// <example>false</example>
    public bool HasAxle { get; set; }

    /// <summary>
    /// Применим к коммерческим автомобилям
    /// </summary>
    /// <example>true</example>
    public bool HasCommercialVehicle { get; set; }

    /// <summary>
    /// Применим к двигателям
    /// </summary>
    /// <example>false</example>
    public bool HasEngine { get; set; }

    /// <summary>
    /// Имеет связанные элементы
    /// </summary>
    /// <example>true</example>
    public bool HasLinkItems { get; set; }

    /// <summary>
    /// Применим к мотоциклам
    /// </summary>
    /// <example>false</example>
    public bool HasMotorbike { get; set; }

    /// <summary>
    /// Применим к легковым автомобилям
    /// </summary>
    /// <example>true</example>
    public bool HasPassengerCar { get; set; }

    /// <summary>
    /// Валидный артикул
    /// </summary>
    /// <example>true</example>
    public bool IsValid { get; set; }
}

/// <summary>
/// Информация о поставщике
/// </summary>
public class SupplierInfoDto
{
    /// <summary>
    /// ID поставщика
    /// </summary>
    /// <example>7</example>
    public ushort Id { get; set; }

    /// <summary>
    /// Описание поставщика
    /// </summary>
    /// <example>BOSCH</example>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Matchcode поставщика
    /// </summary>
    /// <example>BOSCH</example>
    public string Matchcode { get; set; } = string.Empty;

    /// <summary>
    /// Версия данных
    /// </summary>
    /// <example>20240101</example>
    public string? DataVersion { get; set; }

    /// <summary>
    /// Количество артикулов у поставщика
    /// </summary>
    /// <example>50000</example>
    public uint? NbrOfArticles { get; set; }
}

/// <summary>
/// Кросс (аналог) артикула
/// </summary>
public class CrossDto
{
    /// <summary>
    /// ID производителя
    /// </summary>
    /// <example>16</example>
    public ushort ManufacturerId { get; set; }

    /// <summary>
    /// OEM номер
    /// </summary>
    /// <example>123456789</example>
    public string OENbr { get; set; } = string.Empty;

    /// <summary>
    /// Информация о производителе
    /// </summary>
    public ManufacturerDto? Manufacturer { get; set; }
}

/// <summary>
/// Производитель
/// </summary>
public class ManufacturerDto
{
    /// <summary>
    /// ID производителя
    /// </summary>
    /// <example>16</example>
    public ushort Id { get; set; }

    /// <summary>
    /// Описание производителя
    /// </summary>
    /// <example>BMW</example>
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// OEM номер
/// </summary>
public class OeNumberDto
{
    /// <summary>
    /// OEM номер
    /// </summary>
    /// <example>123456789</example>
    public string OENbr { get; set; } = string.Empty;

    /// <summary>
    /// Является добавкой
    /// </summary>
    /// <example>false</example>
    public bool IsAdditive { get; set; }
}

/// <summary>
/// Атрибут артикула
/// </summary>
public class AttributeDto
{
    /// <summary>
    /// ID атрибута
    /// </summary>
    /// <example>1</example>
    public uint Id { get; set; }

    /// <summary>
    /// Описание атрибута
    /// </summary>
    /// <example>Длина</example>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Заголовок для отображения
    /// </summary>
    /// <example>Длина</example>
    public string DisplayTitle { get; set; } = string.Empty;

    /// <summary>
    /// Значение для отображения
    /// </summary>
    /// <example>120 мм</example>
    public string DisplayValue { get; set; } = string.Empty;
}

/// <summary>
/// Изображение артикула
/// </summary>
public class ImageDto
{
    /// <summary>
    /// Имя файла изображения
    /// </summary>
    /// <example>image.jpg</example>
    public string PictureName { get; set; } = string.Empty;

    /// <summary>
    /// Описание изображения
    /// </summary>
    /// <example>Внешний вид</example>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Дополнительное описание
    /// </summary>
    public string? AdditionalDescription { get; set; }

    /// <summary>
    /// Имя документа
    /// </summary>
    public string? DocumentName { get; set; }

    /// <summary>
    /// Тип документа
    /// </summary>
    public string? DocumentType { get; set; }

    /// <summary>
    /// Показывать сразу
    /// </summary>
    /// <example>true</example>
    public bool ShowImmediately { get; set; }
}

/// <summary>
/// Применимость (linkage)
/// </summary>
public class LinkageDto
{
    /// <summary>
    /// Тип применимости
    /// </summary>
    /// <example>P</example>
    public string LinkageTypeId { get; set; } = string.Empty;

    /// <summary>
    /// ID применимости
    /// </summary>
    /// <example>12345</example>
    public uint LinkageId { get; set; }
}

/// <summary>
/// EAN код
/// </summary>
public class EanCodeDto
{
    /// <summary>
    /// EAN код
    /// </summary>
    /// <example>4001512345678</example>
    public string Ean { get; set; } = string.Empty;
}

/// <summary>
/// Дополнительная информация
/// </summary>
public class InformationDto
{
    /// <summary>
    /// Ключ типа информации
    /// </summary>
    /// <example>INFO</example>
    public string InformationTypeKey { get; set; } = string.Empty;

    /// <summary>
    /// Тип информации
    /// </summary>
    /// <example>Описание</example>
    public string InformationType { get; set; } = string.Empty;

    /// <summary>
    /// Текст информации
    /// </summary>
    /// <example>Дополнительная информация об артикуле</example>
    public string InformationText { get; set; } = string.Empty;
}

/// <summary>
/// Аксессуар
/// </summary>
public class AccessoryDto
{
    /// <summary>
    /// ID поставщика аксессуара
    /// </summary>
    /// <example>7</example>
    public ushort AccSupplierId { get; set; }

    /// <summary>
    /// Номер артикула аксессуара
    /// </summary>
    /// <example>ACC-123</example>
    public string AccDataSupplierArticleNumber { get; set; } = string.Empty;

    /// <summary>
    /// Информация о поставщике аксессуара
    /// </summary>
    public SupplierInfoDto? AccSupplier { get; set; }
}

/// <summary>
/// Новый номер
/// </summary>
public class NewNumberDto
{
    /// <summary>
    /// ID нового поставщика
    /// </summary>
    /// <example>7</example>
    public ushort NewSupplierId { get; set; }

    /// <summary>
    /// Новый номер артикула
    /// </summary>
    /// <example>NEW-123</example>
    public string NewDataSupplierArticleNumber { get; set; } = string.Empty;

    /// <summary>
    /// Информация о новом поставщике
    /// </summary>
    public SupplierInfoDto? NewSupplier { get; set; }
}

