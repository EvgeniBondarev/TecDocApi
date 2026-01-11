using TecDocApi.Application.Models;

namespace TecDocApi.Application.Services;

/// <summary>
/// Сервис для работы с Elasticsearch - поиск и индексация поставщиков
/// </summary>
public interface ISupplierElasticsearchService
{
    /// <summary>
    /// Создает индекс в Elasticsearch
    /// </summary>
    Task<bool> CreateIndexAsync();

    /// <summary>
    /// Индексирует одного поставщика
    /// </summary>
    Task<bool> IndexSupplierAsync(SupplierDocument supplier);

    /// <summary>
    /// Массовая индексация поставщиков
    /// </summary>
    Task<bool> BulkIndexSuppliersAsync(IEnumerable<SupplierDocument> suppliers);

    /// <summary>
    /// Поиск поставщиков по запросу
    /// </summary>
    Task<SupplierSearchResult> SearchSuppliersAsync(SupplierSearchRequest request);

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

