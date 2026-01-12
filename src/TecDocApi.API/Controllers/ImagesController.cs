using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCaching;
using TecDocApi.API.Models;
using TecDocApi.Application.Services;

namespace TecDocApi.API.Controllers;

/// <summary>
/// Контроллер для работы с изображениями из S3 хранилища
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ImagesController : ControllerBase
{
    private readonly IS3ImageService _s3ImageService;
    private readonly ILogger<ImagesController> _logger;

    public ImagesController(
        IS3ImageService s3ImageService,
        ILogger<ImagesController> logger)
    {
        _s3ImageService = s3ImageService;
        _logger = logger;
    }

    /// <summary>
    /// Получить URL изображения из S3
    /// </summary>
    /// <remarks>
    /// Возвращает публичный URL изображения из S3 хранилища Timeweb.
    /// 
    /// ### Особенности:
    /// - Быстрый метод с кэшированием URL на 24 часа
    /// - Автоматическая проверка существования изображения
    /// - Поддержка кэширования на стороне клиента (Cache-Control)
    /// 
    /// ### Формат пути в S3:
    /// `{bucket}/{basePath}/{supplierId}/{fileName}`
    /// 
    /// Пример: `25f554fc-.../TD2018/images/101/101_116209_1.jpg`
    /// 
    /// ### Параметры:
    /// - `supplierId` - ID поставщика (1-65535)
    /// - `fileName` - Имя файла изображения (например, "101_116209_1.jpg")
    /// </remarks>
    /// <param name="supplierId">ID поставщика</param>
    /// <param name="fileName">Имя файла изображения</param>
    /// <response code="200">URL изображения успешно получен</response>
    /// <response code="404">Изображение не найдено</response>
    /// <response code="400">Некорректные параметры запроса</response>
    /// <response code="500">Внутренняя ошибка сервера</response>
    [HttpGet("{supplierId}/{fileName}")]
    [ResponseCache(Duration = 86400, VaryByQueryKeys = new[] { "supplierId", "fileName" })] // Кэш на 24 часа
    [ProducesResponseType(typeof(ImageUrlResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetImageUrl(
        [FromRoute] ushort supplierId,
        [FromRoute] string fileName)
    {
        if (supplierId == 0)
        {
            return BadRequest(new ErrorResponse
            {
                Code = ErrorCodes.BAD_REQUEST,
                Message = "ID поставщика должен быть больше 0"
            });
        }

        if (string.IsNullOrWhiteSpace(fileName))
        {
            return BadRequest(new ErrorResponse
            {
                Code = ErrorCodes.BAD_REQUEST,
                Message = "Имя файла не может быть пустым"
            });
        }

        try
        {
            var imageUrl = await _s3ImageService.GetImageUrlAsync(supplierId, fileName);

            if (string.IsNullOrEmpty(imageUrl))
            {
                return NotFound(new ErrorResponse
                {
                    Code = ErrorCodes.NOT_FOUND,
                    Message = $"Изображение '{fileName}' для поставщика {supplierId} не найдено в S3 хранилище"
                });
            }

            return Ok(new ImageUrlResponse
            {
                Url = imageUrl,
                SupplierId = supplierId,
                FileName = fileName
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении URL изображения: SupplierId={SupplierId}, FileName={FileName}", supplierId, fileName);
            return StatusCode(500, new ErrorResponse
            {
                Code = ErrorCodes.INTERNAL_SERVER_ERROR,
                Message = "Внутренняя ошибка сервера при получении изображения"
            });
        }
    }

    /// <summary>
    /// Получить изображение напрямую из S3 (прокси)
    /// </summary>
    /// <remarks>
    /// Возвращает изображение напрямую из S3 хранилища как поток данных.
    /// Используйте этот метод, если нужен прямой доступ к файлу без редиректа.
    /// 
    /// ### Особенности:
    /// - Прямая передача потока данных из S3
    /// - Автоматическое определение Content-Type по расширению файла
    /// - Поддержка кэширования на стороне клиента
    /// 
    /// ### Рекомендации:
    /// Для большинства случаев лучше использовать метод `GetImageUrl`, который возвращает прямой URL к S3.
    /// Этот метод полезен, если нужна дополнительная обработка изображения или проксирование через API.
    /// </remarks>
    /// <param name="supplierId">ID поставщика</param>
    /// <param name="fileName">Имя файла изображения</param>
    /// <response code="200">Изображение успешно получено</response>
    /// <response code="404">Изображение не найдено</response>
    /// <response code="400">Некорректные параметры запроса</response>
    /// <response code="500">Внутренняя ошибка сервера</response>
    [HttpGet("{supplierId}/{fileName}/stream")]
    [ResponseCache(Duration = 86400, VaryByQueryKeys = new[] { "supplierId", "fileName" })] // Кэш на 24 часа
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetImageStream(
        [FromRoute] ushort supplierId,
        [FromRoute] string fileName)
    {
        if (supplierId == 0)
        {
            return BadRequest(new ErrorResponse
            {
                Code = ErrorCodes.BAD_REQUEST,
                Message = "ID поставщика должен быть больше 0"
            });
        }

        if (string.IsNullOrWhiteSpace(fileName))
        {
            return BadRequest(new ErrorResponse
            {
                Code = ErrorCodes.BAD_REQUEST,
                Message = "Имя файла не может быть пустым"
            });
        }

        try
        {
            var imageStream = await _s3ImageService.GetImageStreamAsync(supplierId, fileName);

            if (imageStream == null)
            {
                return NotFound(new ErrorResponse
                {
                    Code = ErrorCodes.NOT_FOUND,
                    Message = $"Изображение '{fileName}' для поставщика {supplierId} не найдено в S3 хранилище"
                });
            }

            // Определяем Content-Type по расширению файла
            var contentType = GetContentType(fileName);
            
            // Устанавливаем заголовки для кэширования
            Response.Headers.CacheControl = "public, max-age=86400"; // 24 часа
            Response.Headers.ETag = $"\"{supplierId}_{fileName}\"";

            return File(imageStream, contentType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении изображения: SupplierId={SupplierId}, FileName={FileName}", supplierId, fileName);
            return StatusCode(500, new ErrorResponse
            {
                Code = ErrorCodes.INTERNAL_SERVER_ERROR,
                Message = "Внутренняя ошибка сервера при получении изображения"
            });
        }
    }

    /// <summary>
    /// Проверить существование изображения в S3
    /// </summary>
    /// <remarks>
    /// Быстрая проверка существования изображения в S3 хранилище без загрузки файла.
    /// 
    /// ### Особенности:
    /// - Быстрый метод с кэшированием результата
    /// - Не загружает файл, только проверяет метаданные
    /// </remarks>
    /// <param name="supplierId">ID поставщика</param>
    /// <param name="fileName">Имя файла изображения</param>
    /// <response code="200">Результат проверки</response>
    /// <response code="400">Некорректные параметры запроса</response>
    [HttpGet("{supplierId}/{fileName}/exists")]
    [ResponseCache(Duration = 3600, VaryByQueryKeys = new[] { "supplierId", "fileName" })] // Кэш на 1 час
    [ProducesResponseType(typeof(ImageExistsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CheckImageExists(
        [FromRoute] ushort supplierId,
        [FromRoute] string fileName)
    {
        if (supplierId == 0)
        {
            return BadRequest(new ErrorResponse
            {
                Code = ErrorCodes.BAD_REQUEST,
                Message = "ID поставщика должен быть больше 0"
            });
        }

        if (string.IsNullOrWhiteSpace(fileName))
        {
            return BadRequest(new ErrorResponse
            {
                Code = ErrorCodes.BAD_REQUEST,
                Message = "Имя файла не может быть пустым"
            });
        }

        try
        {
            var exists = await _s3ImageService.ImageExistsAsync(supplierId, fileName);

            return Ok(new ImageExistsResponse
            {
                Exists = exists,
                SupplierId = supplierId,
                FileName = fileName
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при проверке существования изображения: SupplierId={SupplierId}, FileName={FileName}", supplierId, fileName);
            return StatusCode(500, new ErrorResponse
            {
                Code = ErrorCodes.INTERNAL_SERVER_ERROR,
                Message = "Внутренняя ошибка сервера при проверке изображения"
            });
        }
    }

    /// <summary>
    /// Определяет Content-Type по расширению файла
    /// </summary>
    private static string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".bmp" => "image/bmp",
            ".svg" => "image/svg+xml",
            _ => "application/octet-stream"
        };
    }
}

