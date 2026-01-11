using TecDocApi.Application.Models;

namespace TecDocApi.Application.Services;

/// <summary>
/// Сервис для работы с Elasticsearch - поиск и индексация артикулов
/// </summary>
public interface IArticleElasticsearchService
{
    /// <summary>
    /// Создает индекс в Elasticsearch
    /// </summary>
    Task<bool> CreateIndexAsync();

    /// <summary>
    /// Индексирует один артикул
    /// </summary>
    Task<bool> IndexArticleAsync(ArticleDocument article);

    /// <summary>
    /// Массовая индексация артикулов
    /// </summary>
    Task<bool> BulkIndexArticlesAsync(IEnumerable<ArticleDocument> articles);

    /// <summary>
    /// Поиск артикулов по запросу
    /// </summary>
    Task<ArticleSearchResult> SearchArticlesAsync(ArticleSearchRequest request);

    /// <summary>
    /// Поиск артикулов по модели поставщика (Description и Matchcode)
    /// </summary>
    Task<ArticleSearchResult> SearchArticlesBySupplierAsync(ArticleSearchRequest request);

    /// <summary>
    /// Удаляет индекс
    /// </summary>
    Task<bool> DeleteIndexAsync();

    /// <summary>
    /// Получает общее количество проиндексированных документов
    /// </summary>
    Task<long> GetTotalCountAsync();

    /// <summary>
    /// Проверяет существование индекса
    /// </summary>
    Task<bool> IndexExistsAsync();
}

