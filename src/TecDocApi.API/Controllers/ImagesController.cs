using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TecDocApi.API.Models;
using TecDocApi.Application.Models;
using TecDocApi.Application.Services;
using TecDocApi.Infrastructure.Data;

namespace TecDocApi.API.Controllers;

/// <summary>
/// Контроллер для работы с изображениями из S3 хранилища
/// </summary>
[EnableCors("AllowAll")]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ImagesController : ControllerBase
{
    private readonly IS3ImageService _s3ImageService;
    private readonly IDbContextFactory<TecDocContext> _contextFactory;
    private readonly ILogger<ImagesController> _logger;

    public ImagesController(
        IS3ImageService s3ImageService,
        IDbContextFactory<TecDocContext> contextFactory,
        ILogger<ImagesController> logger)
    {
        _s3ImageService = s3ImageService;
        _contextFactory = contextFactory;
        _logger = logger;
    }

    /// <summary>
    /// Найти картинки по articleNumber с опциональным supplierId
    /// </summary>
    /// <remarks>
    /// Этот метод используется отдельно от Elasticsearch-поиска артикулов.
    /// 
    /// ### Новый рекомендуемый сценарий:
    /// 1. Выполнить `/api/ArticleSearch/search` или `/api/ArticleSearch/search-by-supplier`
    /// 2. Взять из результата supplierId и dataSupplierArticleNumber
    /// 3. Выполнить /api/Images/article-search?supplierId=...&amp;articleNumber=...
    /// 
    /// ### Что делает метод:
    /// - ищет кандидаты во всех папках S3Info без учета регистра и спецсимволов
    /// - умеет находить варианты вроде ALM2019YX-6, ALM2019YX_3, 1234ALM2019YX-6
    /// - учитывает supplierId как фильтр и как фактор ранжирования, если он передан
    /// - проверяет наличие файлов через S3Info/S3
    /// - удаляет дубликаты по PictureName
    /// - возвращает только реально доступные изображения
    /// </remarks>
    /// <param name="supplierId">Опциональный ID поставщика</param>
    /// <param name="articleNumber">Артикул для нестрогого поиска</param>
    /// <param name="maxResults">Максимальное количество результатов</param>
    /// <response code="200">Список доступных изображений для артикула</response>
    /// <response code="400">Некорректные параметры запроса</response>
    /// <response code="500">Внутренняя ошибка сервера</response>
    [HttpGet("article-search")]
    [ProducesResponseType(typeof(List<ArticleImageDocument>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SearchArticleImages(
        [FromQuery] ushort? supplierId,
        [FromQuery] string articleNumber,
        [FromQuery] int maxResults = 20)
    {
        if (supplierId.HasValue && supplierId.Value == 0)
        {
            return BadRequest(new ErrorResponse
            {
                Code = ErrorCodes.BAD_REQUEST,
                Message = "ID поставщика должен быть больше 0"
            });
        }

        if (string.IsNullOrWhiteSpace(articleNumber))
        {
            return BadRequest(new ErrorResponse
            {
                Code = ErrorCodes.BAD_REQUEST,
                Message = "Номер артикула не может быть пустым"
            });
        }

        try
        {
            var result = supplierId.HasValue
                ? await SearchArticleImagesBySupplierMetadataAsync(supplierId.Value, articleNumber, maxResults)
                : new List<ArticleImageDocument>();

            if (result.Count == 0)
            {
                var matches = await _s3ImageService.SearchImagesByArticleAsync(articleNumber, supplierId, maxResults);
                result = matches.Select(match => new ArticleImageDocument
                {
                    PictureName = match.PictureName,
                    Description = match.MatchedBy,
                    AdditionalDescription = match.ObjectKey,
                    DocumentName = match.ObjectKey,
                    DocumentType = Path.GetExtension(match.PictureName).TrimStart('.').ToUpperInvariant(),
                    ShowImmediately = false,
                    Url = match.Url,
                    StreamUrl = match.StreamUrl,
                    S3Url = match.S3Url
                }).ToList();
            }

            result = result.DistinctBy(e => e.Url).ToList();
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при поиске изображений артикула: SupplierId={SupplierId}, ArticleNumber={ArticleNumber}", supplierId, articleNumber);
            return StatusCode(500, new ErrorResponse
            {
                Code = ErrorCodes.INTERNAL_SERVER_ERROR,
                Message = "Внутренняя ошибка сервера при поиске изображений"
            });
        }
    }

    private async Task<List<ArticleImageDocument>> SearchArticleImagesBySupplierMetadataAsync(ushort supplierId, string articleNumber, int maxResults)
    {
        var normalizedArticle = NormalizeForSearch(articleNumber);
        if (string.IsNullOrWhiteSpace(normalizedArticle))
            return [];

        var rawTokens = ExtractTokens(articleNumber);
        var primaryToken = rawTokens.OrderByDescending(token => token.Length).FirstOrDefault() ?? normalizedArticle;

        await using var db = await _contextFactory.CreateDbContextAsync();
        var candidates = await db.ArticleImages
            .AsNoTracking()
            .Where(img => img.SupplierId == supplierId &&
                          (img.DataSupplierArticleNumber.Contains(primaryToken) || img.PictureName.Contains(primaryToken)))
            .OrderByDescending(img => img.ShowImmediately)
            .ThenBy(img => img.PictureName)
            .Select(img => new
            {
                img.SupplierId,
                img.DataSupplierArticleNumber,
                img.PictureName,
                img.Description,
                img.AdditionalDescription,
                img.DocumentName,
                img.DocumentType,
                img.ShowImmediately
            })
            .Take(80)
            .ToListAsync();

        var filtered = candidates
            .Where(candidate => NormalizeForSearch(candidate.DataSupplierArticleNumber).Contains(normalizedArticle, StringComparison.Ordinal)
                             || NormalizeForSearch(candidate.PictureName).Contains(normalizedArticle, StringComparison.Ordinal))
            .GroupBy(candidate => candidate.PictureName, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .Take(Math.Clamp(maxResults, 1, 100))
            .ToList();

        var results = new List<ArticleImageDocument>(filtered.Count);
        foreach (var image in filtered)
        {
            if (!await _s3ImageService.ImageExistsAsync(image.SupplierId, image.PictureName))
                continue;

            var url = await _s3ImageService.GetImageUrlAsync(image.SupplierId, image.PictureName) ?? string.Empty;
            results.Add(new ArticleImageDocument
            {
                PictureName = image.PictureName,
                Description = image.Description,
                AdditionalDescription = image.AdditionalDescription,
                DocumentName = image.DocumentName,
                DocumentType = image.DocumentType,
                ShowImmediately = image.ShowImmediately,
                Url = url,
                StreamUrl = url,
                S3Url = url
            });
        }

        return results;
    }

    private static string NormalizeForSearch(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var builder = new StringBuilder(value.Length);
        foreach (var character in value)
        {
            if (char.IsLetterOrDigit(character))
            {
                builder.Append(char.ToUpperInvariant(character));
            }
        }

        return builder.ToString();
    }

    private static List<string> ExtractTokens(string value)
    {
        return Regex.Split(value.ToUpperInvariant(), "[^A-Z0-9]+")
            .Where(token => !string.IsNullOrWhiteSpace(token))
            .Distinct(StringComparer.Ordinal)
            .ToList();
    }

    /// <summary>
    /// Получить изображение напрямую по object key из S3
    /// </summary>
    /// <param name="objectKey">Полный object key в S3</param>
    /// <response code="200">Изображение успешно получено</response>
    /// <response code="404">Изображение не найдено</response>
    /// <response code="400">Некорректные параметры запроса</response>
    [HttpGet("by-key/stream")]
    public async Task<IActionResult> GetImageStreamByObjectKey([FromQuery] string objectKey)
    {
        if (string.IsNullOrWhiteSpace(objectKey))
        {
            return BadRequest(new ErrorResponse
            {
                Code = ErrorCodes.BAD_REQUEST,
                Message = "objectKey не может быть пустым"
            });
        }

        var imageStream = await _s3ImageService.GetImageStreamByObjectKeyAsync(objectKey);
        if (imageStream == null)
        {
            return NotFound(new ErrorResponse
            {
                Code = ErrorCodes.NOT_FOUND,
                Message = $"Изображение '{objectKey}' не найдено в S3 хранилище"
            });
        }

        Response.Headers.CacheControl = "public, max-age=86400";
        Response.Headers.ETag = $"\"{Convert.ToHexString(Encoding.UTF8.GetBytes(objectKey))}\"";
        return File(imageStream, GetContentType(objectKey));
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
                Url = string.IsNullOrWhiteSpace(imageUrl)
                    ? string.Empty
                    : imageUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || imageUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
                        ? imageUrl
                        : $"{Request.Scheme}://{Request.Host}{imageUrl}",
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

