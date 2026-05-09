namespace TecDocApi.Application.Models;

/// <summary>
/// Документ артикула для индексации в Elasticsearch
/// </summary>
/// <remarks>
/// Представляет артикул с полной информацией для поиска и отображения.
/// Содержит данные артикула и связанного поставщика.
/// </remarks>
public class ArticleDocument
{
    /// <summary>
    /// Уникальный идентификатор документа в Elasticsearch
    /// </summary>
    /// <remarks>
    /// Формат: "{SupplierId}_{DataSupplierArticleNumber}"
    /// Пример: "7_12345"
    /// </remarks>
    /// <example>7_12345</example>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// ID поставщика (производителя)
    /// </summary>
    /// <remarks>
    /// Идентификатор поставщика из таблицы suppliers.
    /// Используется для связи с данными поставщика и фильтрации результатов.
    /// </remarks>
    /// <example>7</example>
    public ushort SupplierId { get; set; }

    /// <summary>
    /// Номер артикула поставщика в оригинальном формате
    /// </summary>
    /// <remarks>
    /// Артикул в том виде, как он указан у поставщика (может содержать дефисы, пробелы и т.д.).
    /// Максимальная длина: 32 символа.
    /// </remarks>
    /// <example>12-345</example>
    public string DataSupplierArticleNumber { get; set; } = string.Empty;

    /// <summary>
    /// Артикул в нормализованном виде для поиска
    /// </summary>
    /// <remarks>
    /// Артикул в поисковом написании: только цифры и буквы, без пробелов, дефисов, точек и других спецсимволов.
    /// Все символы приведены к верхнему регистру.
    /// Используется для точного и частичного поиска (ngram).
    /// Максимальная длина: 64 символа.
    /// </remarks>
    /// <example>12345</example>
    public string FoundString { get; set; } = string.Empty;

    /// <summary>
    /// Основное описание артикула (наименование)
    /// </summary>
    /// <remarks>
    /// Полнотекстовое описание артикула с поддержкой русского языка.
    /// Используется для полнотекстового поиска с учетом морфологии и стемминга.
    /// Максимальная длина: 128 символов.
    /// </remarks>
    /// <example>Фильтр масляный</example>
    public string NormalizedDescription { get; set; } = string.Empty;

    /// <summary>
    /// Дополнительное описание (примечание)
    /// </summary>
    /// <remarks>
    /// Дополнительная информация об артикуле.
    /// Также используется для поиска, но с меньшим приоритетом (boost 1.0).
    /// Максимальная длина: 128 символов.
    /// </remarks>
    /// <example>Для легковых автомобилей</example>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Статус изделия
    /// </summary>
    /// <remarks>
    /// Текущий статус артикула (например, "Normal", "Discontinued", "псевдо-изделие").
    /// </remarks>
    /// <example>Normal</example>
    public string ArticleStateDisplayValue { get; set; } = string.Empty;

    /// <summary>
    /// Количество единиц в упаковке
    /// </summary>
    /// <remarks>
    /// Количество артикулов в одной упаковке.
    /// Может быть null, если информация недоступна.
    /// </remarks>
    /// <example>1</example>
    public uint? QuantityPerPackingUnit { get; set; }

    /// <summary>
    /// Название поставщика (описание)
    /// </summary>
    /// <remarks>
    /// Полное название поставщика из таблицы suppliers.
    /// Используется для поиска артикулов по модели поставщика.
    /// </remarks>
    /// <example>BOSCH</example>
    public string SupplierDescription { get; set; } = string.Empty;

    /// <summary>
    /// Код поставщика для поиска (Matchcode)
    /// </summary>
    /// <remarks>
    /// Короткий код поставщика для быстрого поиска.
    /// Используется для поиска артикулов по модели поставщика.
    /// </remarks>
    /// <example>BOS</example>
    public string SupplierMatchcode { get; set; } = string.Empty;

    /// <summary>
    /// Дата и время индексации документа в Elasticsearch
    /// </summary>
    /// <remarks>
    /// UTC время, когда документ был проиндексирован в Elasticsearch.
    /// Используется для отслеживания актуальности данных.
    /// </remarks>
    /// <example>2024-01-10T12:00:00Z</example>
    public DateTime IndexedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Дата и время последнего обновления артикула в базе данных
    /// </summary>
    /// <remarks>
    /// UTC время последнего изменения артикула в MySQL.
    /// Может быть null для старых записей.
    /// </remarks>
    /// <example>2024-01-10T12:00:00Z</example>
    public DateTime? LastModified { get; set; }

    /// <summary>
    /// Список изображений артикула, подмешиваемый в API-ответ после поиска.
    /// В Elasticsearch не используется как поисковое поле.
    /// </summary>
    public List<ArticleImageDocument>? Images { get; set; }
}

public class ArticleImageDocument
{
    public string PictureName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string AdditionalDescription { get; set; } = string.Empty;

    public string DocumentName { get; set; } = string.Empty;

    public string DocumentType { get; set; } = string.Empty;

    public bool ShowImmediately { get; set; }

    public string Url { get; set; } = string.Empty;

    public string StreamUrl { get; set; } = string.Empty;

    public string S3Url { get; set; } = string.Empty;
}

public class S3ImageSearchResult
{
    public string PictureName { get; set; } = string.Empty;

    public string ObjectKey { get; set; } = string.Empty;

    public ushort? SupplierId { get; set; }

    public int MatchScore { get; set; }

    public string MatchedBy { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;

    public string StreamUrl { get; set; } = string.Empty;

    public string S3Url { get; set; } = string.Empty;
}

