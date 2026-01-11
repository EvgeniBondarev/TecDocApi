using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.ComponentModel.DataAnnotations;
using TecDocApi.API.DTOs;
using TecDocApi.API.Exceptions;
using TecDocApi.API.Models;
using TecDocApi.Application.Services;

namespace TecDocApi.API.Controllers;

/// <summary>
/// API для работы с артикулами TecDoc
/// </summary>
[EnableCors("AllowAll")]
[ApiController]
[Route("api/v1/articles")]
[Tags("Articles")]
[Produces("application/json")]
public class TecDocArticlesController : ControllerBase
{
    private readonly ITecDocArticleService _articleService;
    private readonly ILogger<TecDocArticlesController> _logger;

    public TecDocArticlesController(ITecDocArticleService articleService, ILogger<TecDocArticlesController> logger)
    {
        _articleService = articleService;
        _logger = logger;
    }

    /// <summary>
    /// Поиск артикулов по номеру
    /// </summary>
    /// <remarks>
    /// 🔍 **Поиск без учёта:**
    /// - регистра (ABC-123 = abc-123)
    /// - пробелов (ABC 123 = ABC123)
    /// - спецсимволов (ABC-123 = ABC123)
    ///
    /// ### Примеры:
    /// - `ABC-123` = `abc 123` = `ABC123`
    /// - `12345` найдет все артикулы с нормализованным номером "12345"
    ///
    /// ### Ограничения:
    /// - Максимум 50 результатов
    /// - Таймаут запроса: 10 секунд
    /// - Rate limit: 50 запросов за 10 секунд
    /// - Кэш: 5 минут
    /// </remarks>
    /// <param name="articleNumber">Артикул для поиска (будет нормализован автоматически)</param>
    /// <param name="supplierId">Опционально: ID поставщика для фильтрации результатов</param>
    /// <response code="200">Артикулы найдены</response>
    /// <response code="400">Ошибка валидации параметров</response>
    /// <response code="404">Артикулы не найдены</response>
    /// <response code="429">Превышен лимит запросов</response>
    /// <response code="500">Внутренняя ошибка сервера</response>
    [HttpGet("search")]
    [EnableRateLimiting("search")]
    [ResponseCache(Duration = 300, VaryByQueryKeys = new[] { "articleNumber", "supplierId" })]
    [ProducesResponseType(typeof(ArticleSearchResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SearchByArticle(
        [FromQuery] [Required] [StringLength(100, MinimumLength = 1)] string articleNumber, 
        [FromQuery] ushort? supplierId = null, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(articleNumber))
        {
            return BadRequest("Артикул не может быть пустым");
        }

        try
        {
            var result = await _articleService.SearchByArticleAsync(articleNumber, supplierId, cancellationToken);
            
            if (result is { } r && r.GetType().GetProperty("Count")?.GetValue(r) is int count && count == 0)
            {
                return NotFound($"Артикул '{articleNumber}' не найден в базе TecDoc");
            }

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при поиске артикула {ArticleNumber}", articleNumber);
            return StatusCode(500, $"Ошибка при поиске артикула: {ex.Message}");
        }
    }

    /// <summary>
    /// Получить артикул по точному совпадению
    /// </summary>
    /// <remarks>
    /// Получение полной информации об артикуле по точному совпадению FoundString (нормализованный артикул).
    ///
    /// ### Особенности:
    /// - Артикул будет автоматически нормализован
    /// - Возвращает полную информацию со всеми связями (кроссы, OEM, изображения и т.д.)
    ///
    /// ### Ограничения:
    /// - Таймаут запроса: 10 секунд
    /// - Rate limit: 50 запросов за 10 секунд
    /// - Кэш: 10 минут
    /// </remarks>
    /// <param name="supplierId">ID поставщика</param>
    /// <param name="articleNumber">Номер артикула (будет нормализован)</param>
    /// <response code="200">Артикул найден</response>
    /// <response code="404">Артикул не найден</response>
    /// <response code="429">Превышен лимит запросов</response>
    /// <response code="500">Внутренняя ошибка сервера</response>
    [HttpGet("{supplierId}/{articleNumber}")]
    [EnableRateLimiting("search")]
    [ResponseCache(Duration = 600)]
    [ProducesResponseType(typeof(ArticleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetByExactMatch(
        [FromRoute] ushort supplierId, 
        [FromRoute] [StringLength(100, MinimumLength = 1)] string articleNumber, 
        CancellationToken cancellationToken = default)
    {
        var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        
        try
        {
            var result = await _articleService.GetByExactMatchAsync(supplierId, articleNumber, cancellationToken);
            
            if (result is { } r && r.GetType().GetProperty("Article")?.GetValue(r) == null)
            {
                _logger.LogInformation("Получение артикула {SupplierId}/{ArticleNumber} от IP {ClientIp}: не найдено", supplierId, articleNumber, clientIp);
                throw new NotFoundException(ErrorCodes.ARTICLE_NOT_FOUND, $"Артикул '{articleNumber}' для поставщика {supplierId} не найден");
            }

            _logger.LogInformation("Получение артикула {SupplierId}/{ArticleNumber} от IP {ClientIp}: успешно", supplierId, articleNumber, clientIp);
            return Ok(result);
        }
        catch (ApiException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении артикула {SupplierId}/{ArticleNumber} от IP {ClientIp}", supplierId, articleNumber, clientIp);
            throw;
        }
    }
}

