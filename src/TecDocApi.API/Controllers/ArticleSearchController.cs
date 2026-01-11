using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;
using TecDocApi.API.Examples;
using TecDocApi.API.Models;
using TecDocApi.Application.Models;
using TecDocApi.Application.Services;

namespace TecDocApi.API.Controllers;

/// <summary>
/// Контроллер для поиска артикулов через Elasticsearch
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ArticleSearchController : ControllerBase
{
    private readonly IArticleElasticsearchService _elasticsearchService;
    private readonly ILogger<ArticleSearchController> _logger;

    public ArticleSearchController(
        IArticleElasticsearchService elasticsearchService,
        ILogger<ArticleSearchController> logger)
    {
        _elasticsearchService = elasticsearchService;
        _logger = logger;
    }

    /// <summary>
    /// Поиск артикулов через Elasticsearch
    /// </summary>
    /// <remarks>
    /// Быстрый полнотекстовый поиск артикулов по полям FoundString и NormalizedDescription.
    /// 
    /// ### Особенности поиска:
    /// - Поиск по FoundString: точное совпадение и частичное совпадение (ngram)
    /// - Поиск по NormalizedDescription: полнотекстовый поиск с поддержкой русского языка
    /// - Поддержка нечеткого поиска (fuzzy search)
    /// - Сортировка по релевантности
    /// 
    /// ### Параметры:
    /// - `query` - поисковый запрос (поиск по FoundString и NormalizedDescription)
    /// - `supplierId` - фильтр по ID поставщика (опционально)
    /// - `page` - номер страницы (по умолчанию 1)
    /// - `pageSize` - размер страницы (по умолчанию 20, максимум 100)
    /// - `sortBy` - сортировка: "relevance" (по умолчанию), "foundString", "description"
    /// </remarks>
    /// <param name="request">Параметры поиска</param>
    /// <response code="200">Поиск выполнен успешно</response>
    /// <response code="400">Некорректные параметры запроса</response>
    /// <response code="500">Внутренняя ошибка сервера</response>
    [HttpPost("search")]
    [SwaggerRequestExample(typeof(ArticleSearchRequest), typeof(ArticleSearchRequestExample))]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(ArticleSearchResultExample))]
    [ProducesResponseType(typeof(ArticleSearchResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SearchArticles([FromBody] ArticleSearchRequest request)
    {
        try
        {
            if (request.PageSize > 100)
            {
                return BadRequest(new ErrorResponse
                {
                    Code = ErrorCodes.BAD_REQUEST,
                    Message = "Максимальный размер страницы: 100"
                });
            }

            var result = await _elasticsearchService.SearchArticlesAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при поиске артикулов");
            return StatusCode(500, new ErrorResponse
            {
                Code = ErrorCodes.INTERNAL_SERVER_ERROR,
                Message = "Внутренняя ошибка сервера при выполнении поиска"
            });
        }
    }

    /// <summary>
    /// Поиск артикулов по модели поставщика через Elasticsearch
    /// </summary>
    /// <remarks>
    /// Быстрый полнотекстовый поиск артикулов по полям SupplierDescription и SupplierMatchcode (модель поставщика).
    /// 
    /// ### Особенности поиска:
    /// - Поиск по SupplierMatchcode: точное совпадение и частичное совпадение (ngram)
    /// - Поиск по SupplierDescription: полнотекстовый поиск с поддержкой русского языка
    /// - Поддержка нечеткого поиска (fuzzy search)
    /// - Сортировка по релевантности
    /// 
    /// ### Параметры:
    /// - `query` - поисковый запрос (поиск по SupplierDescription и SupplierMatchcode)
    /// - `supplierId` - фильтр по ID поставщика (опционально)
    /// - `page` - номер страницы (по умолчанию 1)
    /// - `pageSize` - размер страницы (по умолчанию 20, максимум 100)
    /// - `sortBy` - сортировка: "relevance" (по умолчанию), "foundString", "description"
    /// </remarks>
    /// <param name="request">Параметры поиска</param>
    /// <response code="200">Поиск выполнен успешно</response>
    /// <response code="400">Некорректные параметры запроса</response>
    /// <response code="500">Внутренняя ошибка сервера</response>
    [HttpPost("search-by-supplier")]
    [SwaggerRequestExample(typeof(ArticleSearchRequest), typeof(ArticleSearchRequestExample))]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(ArticleSearchResultExample))]
    [ProducesResponseType(typeof(ArticleSearchResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SearchArticlesBySupplier([FromBody] ArticleSearchRequest request)
    {
        try
        {
            if (request.PageSize > 100)
            {
                return BadRequest(new ErrorResponse
                {
                    Code = ErrorCodes.BAD_REQUEST,
                    Message = "Максимальный размер страницы: 100"
                });
            }

            var result = await _elasticsearchService.SearchArticlesBySupplierAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при поиске артикулов по поставщику");
            return StatusCode(500, new ErrorResponse
            {
                Code = ErrorCodes.INTERNAL_SERVER_ERROR,
                Message = "Внутренняя ошибка сервера при выполнении поиска по поставщику"
            });
        }
    }

    /// <summary>
    /// Проверка состояния Elasticsearch
    /// </summary>
    /// <remarks>
    /// Возвращает информацию о состоянии индекса Elasticsearch и количестве проиндексированных документов.
    /// </remarks>
    /// <response code="200">Elasticsearch доступен</response>
    /// <response code="503">Elasticsearch недоступен</response>
    [HttpGet("health")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> HealthCheck()
    {
        try
        {
            var exists = await _elasticsearchService.IndexExistsAsync();
            var count = await _elasticsearchService.GetTotalCountAsync();
            
            return Ok(new
            {
                status = "healthy",
                indexExists = exists,
                indexedDocuments = count,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed");
            return StatusCode(503, new ErrorResponse
            {
                Code = ErrorCodes.INTERNAL_SERVER_ERROR,
                Message = "Elasticsearch недоступен"
            });
        }
    }
}

