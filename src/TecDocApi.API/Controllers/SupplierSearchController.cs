using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;
using TecDocApi.API.Examples;
using TecDocApi.API.Models;
using TecDocApi.Application.Models;
using TecDocApi.Application.Services;

namespace TecDocApi.API.Controllers;

/// <summary>
/// Контроллер для поиска поставщиков через Elasticsearch
/// </summary>
[EnableCors("AllowAll")]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class SupplierSearchController : ControllerBase
{
    private readonly ISupplierElasticsearchService _elasticsearchService;
    private readonly ILogger<SupplierSearchController> _logger;

    public SupplierSearchController(
        ISupplierElasticsearchService elasticsearchService,
        ILogger<SupplierSearchController> logger)
    {
        _elasticsearchService = elasticsearchService;
        _logger = logger;
    }

    /// <summary>
    /// Поиск поставщиков через Elasticsearch
    /// </summary>
    /// <remarks>
    /// Быстрый полнотекстовый поиск поставщиков по полям Description и Matchcode.
    /// 
    /// ### Особенности поиска:
    /// - Поиск по Matchcode: точное совпадение и частичное совпадение (ngram)
    /// - Поиск по Description: полнотекстовый поиск с поддержкой русского языка
    /// - Поддержка нечеткого поиска (fuzzy search)
    /// - Сортировка по релевантности, описанию, коду или количеству артикулов
    /// 
    /// ### Параметры:
    /// - `query` - поисковый запрос (поиск по Description и Matchcode)
    /// - `page` - номер страницы (по умолчанию 1)
    /// - `pageSize` - размер страницы (по умолчанию 20, максимум 100)
    /// - `sortBy` - сортировка: "relevance" (по умолчанию), "description", "matchcode", "nbrOfArticles"
    /// </remarks>
    /// <param name="request">Параметры поиска</param>
    /// <response code="200">Поиск выполнен успешно</response>
    /// <response code="400">Некорректные параметры запроса</response>
    /// <response code="500">Внутренняя ошибка сервера</response>
    [HttpPost("search")]
    [SwaggerRequestExample(typeof(SupplierSearchRequest), typeof(SupplierSearchRequestExample))]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(SupplierSearchResultExample))]
    [ProducesResponseType(typeof(SupplierSearchResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SearchSuppliers([FromBody] SupplierSearchRequest request)
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

            var result = await _elasticsearchService.SearchSuppliersAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при поиске поставщиков");
            return StatusCode(500, new ErrorResponse
            {
                Code = ErrorCodes.INTERNAL_SERVER_ERROR,
                Message = "Внутренняя ошибка сервера при выполнении поиска"
            });
        }
    }

    /// <summary>
    /// Проверка состояния Elasticsearch для поставщиков
    /// </summary>
    /// <remarks>
    /// Возвращает информацию о состоянии индекса Elasticsearch и количестве проиндексированных поставщиков.
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

