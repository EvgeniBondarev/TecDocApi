using System.ComponentModel.DataAnnotations;

namespace TecDocApi.Application.Models;

/// <summary>
/// Запрос на поиск артикулов через Elasticsearch
/// </summary>
/// <remarks>
/// Используется для быстрого полнотекстового поиска артикулов по полям FoundString и NormalizedDescription.
/// Поддерживает частичный поиск (ngram), нечеткий поиск (fuzzy search) и сортировку по релевантности.
/// </remarks>
public class ArticleSearchRequest
{
    /// <summary>
    /// Поисковый запрос (по FoundString и NormalizedDescription)
    /// </summary>
    /// <remarks>
    /// Поиск выполняется по следующим полям:
    /// - FoundString: точное совпадение (boost 5.0) и частичное совпадение через ngram (boost 3.0)
    /// - NormalizedDescription: полнотекстовый поиск с поддержкой русского языка (boost 2.0)
    /// - Description: дополнительное описание (boost 1.0)
    /// 
    /// Примеры:
    /// - "12345" - найдет артикулы с номером "12345"
    /// - "123" - найдет артикулы, содержащие "123" (например, "12345", "A123", "1234")
    /// - "фильтр масляный" - найдет артикулы с описанием, содержащим эти слова
    /// </remarks>
    /// <example>12345</example>
    public string? Query { get; set; }

    /// <summary>
    /// Режим поиска: auto, article или name
    /// </summary>
    /// <remarks>
    /// - auto: режим определяется автоматически по запросу
    /// - article: приоритет точному и нормализованному поиску по артикулу
    /// - name: приоритет полнотекстовому поиску по названию детали
    /// </remarks>
    /// <example>auto</example>
    public string? SearchMode { get; set; } = "auto";

    /// <summary>
    /// Номер страницы (начиная с 1)
    /// </summary>
    /// <remarks>
    /// Используется для пагинации результатов. По умолчанию: 1
    /// Минимальное значение: 1
    /// </remarks>
    /// <example>1</example>
    [Range(1, int.MaxValue, ErrorMessage = "Номер страницы должен быть больше 0")]
    public int? Page { get; set; } = 1;

    /// <summary>
    /// Размер страницы (количество результатов на странице)
    /// </summary>
    /// <remarks>
    /// Максимальное значение: 100. По умолчанию: 20
    /// Минимальное значение: 1
    /// </remarks>
    /// <example>20</example>
    [Range(1, 100, ErrorMessage = "Размер страницы должен быть от 1 до 100")]
    public int? PageSize { get; set; } = 20;

    /// <summary>
    /// ID поставщика для фильтрации результатов (опционально)
    /// </summary>
    /// <remarks>
    /// Если указан, будут возвращены только артикулы указанного поставщика.
    /// Полезно для сужения области поиска.
    /// Минимальное значение: 1
    /// </remarks>
    /// <example>7</example>
    [Range(1, ushort.MaxValue, ErrorMessage = "ID поставщика должен быть больше 0")]
    public ushort? SupplierId { get; set; }

    /// <summary>
    /// Тип сортировки результатов
    /// </summary>
    /// <remarks>
    /// Доступные значения:
    /// - "relevance" (по умолчанию) - сортировка по релевантности поиска
    /// - "foundString" - сортировка по номеру артикула
    /// - "description" - сортировка по описанию
    /// </remarks>
    /// <example>relevance</example>
    public string? SortBy { get; set; } = "relevance";

    /// <summary>
    /// Направление сортировки
    /// </summary>
    /// <remarks>
    /// false - по возрастанию (A-Z, 0-9)
    /// true - по убыванию (Z-A, 9-0)
    /// </remarks>
    /// <example>false</example>
    public bool SortDescending { get; set; } = false;

    /// <summary>
    /// Количество пропускаемых записей (вычисляется автоматически)
    /// </summary>
    /// <remarks>
    /// Это поле вычисляется автоматически на основе Page и PageSize.
    /// Не нужно указывать в запросе.
    /// </remarks>
    public int Skip => ((Page ?? 1) - 1) * (PageSize ?? 20);
}

