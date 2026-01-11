namespace TecDocApi.Application.Models;

/// <summary>
/// Документ поставщика для индексации в Elasticsearch
/// </summary>
/// <remarks>
/// Представляет поставщика с полной информацией для поиска и отображения.
/// Содержит основные данные поставщика и статистику по артикулам.
/// </remarks>
public class SupplierDocument
{
    /// <summary>
    /// Уникальный идентификатор документа в Elasticsearch
    /// </summary>
    /// <remarks>
    /// Формат: строка с ID поставщика.
    /// Пример: "7"
    /// </remarks>
    /// <example>7</example>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Идентификатор поставщика
    /// </summary>
    /// <remarks>
    /// Уникальный числовой идентификатор поставщика из таблицы suppliers.
    /// Используется для связи с данными артикулов и фильтрации.
    /// </remarks>
    /// <example>7</example>
    public ushort SupplierId { get; set; }

    /// <summary>
    /// Полное название поставщика (описание)
    /// </summary>
    /// <remarks>
    /// Полное название производителя запчастей.
    /// Используется для полнотекстового поиска с поддержкой русского языка.
    /// Максимальная длина: 32 символа.
    /// </remarks>
    /// <example>BOSCH</example>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Код поставщика для поиска (Matchcode)
    /// </summary>
    /// <remarks>
    /// Короткий код поставщика для быстрого поиска и идентификации.
    /// Используется для точного и частичного поиска (ngram).
    /// Максимальная длина: 32 символа.
    /// </remarks>
    /// <example>BOS</example>
    public string Matchcode { get; set; } = string.Empty;

    /// <summary>
    /// Версия данных поставщика
    /// </summary>
    /// <remarks>
    /// Номер версии данных поставщика в каталоге TecDoc.
    /// Используется для отслеживания актуальности данных.
    /// </remarks>
    /// <example>2024</example>
    public ushort? DataVersion { get; set; }

    /// <summary>
    /// Количество артикулов у поставщика в каталоге
    /// </summary>
    /// <remarks>
    /// Общее количество артикулов, доступных у данного поставщика.
    /// Полезно для сортировки поставщиков по популярности/размеру каталога.
    /// Может быть null, если информация недоступна.
    /// </remarks>
    /// <example>15000</example>
    public uint? NbrOfArticles { get; set; }

    /// <summary>
    /// Наличие артикулов новой версии
    /// </summary>
    /// <remarks>
    /// Указывает, есть ли у поставщика артикулы новой версии данных.
    /// true - есть артикулы новой версии
    /// false - нет артикулов новой версии
    /// null - информация недоступна
    /// </remarks>
    /// <example>true</example>
    public bool? HasNewVersionArticles { get; set; }

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
    /// Дата и время последнего обновления поставщика в базе данных
    /// </summary>
    /// <remarks>
    /// UTC время последнего изменения данных поставщика в MySQL.
    /// Может быть null для старых записей.
    /// </remarks>
    /// <example>2024-01-10T12:00:00Z</example>
    public DateTime? LastModified { get; set; }
}

