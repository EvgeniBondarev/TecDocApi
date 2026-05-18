using System.ComponentModel.DataAnnotations;

namespace TecDocApi.Application.Models;

/// <summary>
/// Запрос на поиск поставщиков через Elasticsearch
/// </summary>
/// <remarks>
/// Используется для быстрого полнотекстового поиска поставщиков по полям Description и Matchcode.
/// Поддерживает частичный поиск (ngram), нечеткий поиск (fuzzy search) и различные варианты сортировки.
/// </remarks>
public class SupplierSearchRequest
{
    /// <summary>
    /// Поисковый запрос (по Description и Matchcode)
    /// </summary>
    /// <remarks>
    /// Поиск выполняется по следующим полям:
    /// - Matchcode: точное совпадение (boost 5.0) и частичное совпадение через ngram (boost 4.0)
    /// - Description: полнотекстовый поиск с поддержкой русского языка (boost 3.0)
    /// 
    /// Примеры:
    /// - "BOSCH" - найдет поставщика с названием "BOSCH"
    /// - "BOS" - найдет поставщиков, код которых содержит "BOS" (например, "BOSCH")
    /// - "бош" - найдет поставщиков с описанием, содержащим это слово (с учетом морфологии)
    /// </remarks>
    /// <example>BOSCH</example>
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
    /// Тип сортировки результатов
    /// </summary>
    /// <remarks>
    /// Доступные значения:
    /// - "relevance" (по умолчанию) - сортировка по релевантности поиска
    /// - "description" - сортировка по описанию поставщика
    /// - "matchcode" - сортировка по коду поставщика
    /// - "nbrOfArticles" - сортировка по количеству артикулов у поставщика
    /// </remarks>
    /// <example>relevance</example>
    public string? SortBy { get; set; } = "relevance";

    /// <summary>
    /// Направление сортировки
    /// </summary>
    /// <remarks>
    /// false - по возрастанию (A-Z, 0-9)
    /// true - по убыванию (Z-A, 9-0)
    /// 
    /// Особенно полезно при сортировке по nbrOfArticles для получения поставщиков с наибольшим количеством артикулов.
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

