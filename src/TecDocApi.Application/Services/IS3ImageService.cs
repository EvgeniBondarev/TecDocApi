using TecDocApi.Application.Models;

namespace TecDocApi.Application.Services;

/// <summary>
/// Сервис для работы с изображениями в S3 хранилище
/// </summary>
public interface IS3ImageService
{
    /// <summary>
    /// Ищет изображения по артикулу в S3Info/S3 с умным нестрогим сопоставлением.
    /// </summary>
    /// <param name="articleNumber">Артикул для поиска</param>
    /// <param name="supplierId">Опциональный ID поставщика для сужения и ранжирования результата</param>
    /// <param name="maxResults">Максимальное количество результатов</param>
    /// <returns>Список найденных объектов S3</returns>
    Task<IReadOnlyList<S3ImageSearchResult>> SearchImagesByArticleAsync(string articleNumber, ushort? supplierId = null, int maxResults = 20);
}

