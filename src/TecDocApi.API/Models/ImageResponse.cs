namespace TecDocApi.API.Models;

/// <summary>
/// Ответ с URL изображения
/// </summary>
public class ImageUrlResponse
{
    /// <summary>
    /// URL изображения в S3 хранилище
    /// </summary>
    /// <example>https://s3.timeweb.cloud/25f554fc-.../TD2018/images/101/101_116209_1.jpg</example>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// ID поставщика
    /// </summary>
    /// <example>101</example>
    public ushort SupplierId { get; set; }

    /// <summary>
    /// Имя файла изображения
    /// </summary>
    /// <example>101_116209_1.jpg</example>
    public string FileName { get; set; } = string.Empty;
}

/// <summary>
/// Ответ с результатом проверки существования изображения
/// </summary>
public class ImageExistsResponse
{
    /// <summary>
    /// Существует ли изображение в S3
    /// </summary>
    /// <example>true</example>
    public bool Exists { get; set; }

    /// <summary>
    /// ID поставщика
    /// </summary>
    /// <example>101</example>
    public ushort SupplierId { get; set; }

    /// <summary>
    /// Имя файла изображения
    /// </summary>
    /// <example>101_116209_1.jpg</example>
    public string FileName { get; set; } = string.Empty;
}

