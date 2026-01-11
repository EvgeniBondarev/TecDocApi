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

/// <summary>
/// Результат поиска артикулов через Elasticsearch
/// </summary>
/// <remarks>
/// Содержит список найденных артикулов с информацией о пагинации и производительности запроса.
/// </remarks>
public class ArticleSearchResult
{
    /// <summary>
    /// Список найденных артикулов
    /// </summary>
    /// <remarks>
    /// Каждый элемент содержит полную информацию об артикуле, включая данные поставщика.
    /// Количество элементов соответствует PageSize (или меньше, если это последняя страница).
    /// </remarks>
    public List<ArticleDocument> Items { get; set; } = new();

    /// <summary>
    /// Общее количество найденных записей (без учета пагинации)
    /// </summary>
    /// <remarks>
    /// Это общее количество артикулов, соответствующих поисковому запросу.
    /// Используется для расчета TotalPages и отображения информации пользователю.
    /// </remarks>
    /// <example>150</example>
    public long Total { get; set; }

    /// <summary>
    /// Номер текущей страницы
    /// </summary>
    /// <remarks>
    /// Соответствует значению Page из запроса. Начинается с 1.
    /// </remarks>
    /// <example>1</example>
    public int Page { get; set; }

    /// <summary>
    /// Размер страницы (количество результатов на странице)
    /// </summary>
    /// <remarks>
    /// Соответствует значению PageSize из запроса. Максимум: 100.
    /// </remarks>
    /// <example>20</example>
    public int PageSize { get; set; }

    /// <summary>
    /// Общее количество страниц с результатами
    /// </summary>
    /// <remarks>
    /// Вычисляется как: ceil(Total / PageSize)
    /// Используется для навигации по страницам результатов.
    /// </remarks>
    /// <example>8</example>
    public int TotalPages { get; set; }

    /// <summary>
    /// Время выполнения запроса в Elasticsearch (в миллисекундах)
    /// </summary>
    /// <remarks>
    /// Показывает, сколько времени занял поиск в Elasticsearch.
    /// Полезно для мониторинга производительности.
    /// </remarks>
    /// <example>15</example>
    public long Took { get; set; }
}
