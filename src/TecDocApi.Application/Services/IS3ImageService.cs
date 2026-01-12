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
}

