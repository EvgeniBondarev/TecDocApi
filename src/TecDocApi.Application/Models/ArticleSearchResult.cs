namespace TecDocApi.Application.Models;

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

