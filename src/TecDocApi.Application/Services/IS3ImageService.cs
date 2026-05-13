using TecDocApi.Application.Models;

namespace TecDocApi.Application.Services;

/// <summary>
/// Сервис для работы с изображениями в S3 хранилище
/// </summary>
public interface IS3ImageService
{
    /// <summary>
    /// Получает URL изображения из S3
    /// </summary>
    /// <param name="supplierId">ID поставщика</param>
    /// <param name="fileName">Имя файла изображения</param>
    /// <returns>URL изображения или null, если изображение не найдено</returns>
    Task<string?> GetImageUrlAsync(ushort supplierId, string fileName);

    /// <summary>
    /// Получает изображение как поток данных из S3
    /// </summary>
    /// <param name="supplierId">ID поставщика</param>
    /// <param name="fileName">Имя файла изображения</param>
    /// <returns>Поток данных изображения или null, если изображение не найдено</returns>
    Task<Stream?> GetImageStreamAsync(ushort supplierId, string fileName);

    /// <summary>
    /// Проверяет существование изображения в S3
    /// </summary>
    /// <param name="supplierId">ID поставщика</param>
    /// <param name="fileName">Имя файла изображения</param>
    /// <returns>true, если изображение существует, иначе false</returns>
    Task<bool> ImageExistsAsync(ushort supplierId, string fileName);

    /// <summary>
    /// Ищет изображения по артикулу в S3Info/S3 с умным нестрогим сопоставлением.
    /// </summary>
    /// <param name="articleNumber">Артикул для поиска</param>
    /// <param name="supplierId">Опциональный ID поставщика для сужения и ранжирования результата</param>
    /// <param name="maxResults">Максимальное количество результатов</param>
    /// <returns>Список найденных объектов S3</returns>
    Task<IReadOnlyList<S3ImageSearchResult>> SearchImagesByArticleAsync(string articleNumber, ushort? supplierId = null, int maxResults = 20);

    /// <summary>
    /// Проверяет существование изображения в S3 и возвращает его URL за один вызов.
    /// Объединяет ImageExistsAsync + GetImageUrlAsync, избегая двойного обращения к S3.
    /// </summary>
    /// <param name="supplierId">ID поставщика</param>
    /// <param name="fileName">Имя файла изображения</param>
    /// <returns>URL изображения или null, если изображение не найдено</returns>
    Task<string?> TryGetImageUrlAsync(ushort supplierId, string fileName);

    /// <summary>
    /// Получает поток изображения по полному object key в S3.
    /// </summary>
    /// <param name="objectKey">Полный object key в бакете</param>
    /// <returns>Поток изображения или null</returns>
    Task<Stream?> GetImageStreamByObjectKeyAsync(string objectKey);
}

