using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TecDocApi.API.Models;
using TecDocApi.Application.Models;
using TecDocApi.Application.Options;
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
    private readonly S3Options _s3Options;

    public ImagesController(
        IS3ImageService s3ImageService,
        IDbContextFactory<TecDocContext> contextFactory,
        ILogger<ImagesController> logger,
        IOptions<S3Options> s3Options)
    {
        _s3ImageService = s3ImageService;
        _contextFactory = contextFactory;
        _logger = logger;
        _s3Options = s3Options.Value;
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
    public async Task<IActionResult> SearchArticleImages([FromQuery] ushort? supplierId, [FromQuery] string articleNumber, [FromQuery] int maxResults = 20)
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
                : [];

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
                    S3Url = match.S3Url
                }).ToList();
            }

            result = result.DistinctBy(e => e.S3Url).ToList();
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
            .Where(img => img.SupplierId == supplierId && (img.DataSupplierArticleNumber.Contains(primaryToken) || img.PictureName.Contains(primaryToken)))
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
            .Where(candidate =>
                NormalizeForSearch(candidate.DataSupplierArticleNumber).Contains(normalizedArticle, StringComparison.Ordinal) ||
                NormalizeForSearch(candidate.PictureName).Contains(normalizedArticle, StringComparison.Ordinal))
            .GroupBy(candidate => candidate.PictureName, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .Take(Math.Clamp(maxResults, 1, 100))
            .ToList();

        if (filtered.Count == 0)
            return [];

        return filtered.Select(image =>
            {
                var extension = image.PictureName.Split('.')[^1].ToLower();
                var pictureName = image.PictureName.Replace(image.PictureName.Split('.')[^1], string.Empty);
                var s3Url = $"{_s3Options.EndpointUrl}/{_s3Options.BucketName}/{_s3Options.BasePath}/{supplierId}/{pictureName}jpg";
                return new ArticleImageDocument
                {
                    PictureName = image.PictureName,
                    Description = image.Description,
                    AdditionalDescription = image.AdditionalDescription,
                    DocumentName = image.DocumentName,
                    DocumentType = image.DocumentType,
                    ShowImmediately = image.ShowImmediately,
                    S3Url = s3Url
                };
            }).ToList();
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
}

