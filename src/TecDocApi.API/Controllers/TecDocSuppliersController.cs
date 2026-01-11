using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.ComponentModel.DataAnnotations;
using TecDocApi.API.DTOs;
using TecDocApi.API.Exceptions;
using TecDocApi.API.Models;
using ValidationException = TecDocApi.API.Exceptions.ValidationException;
using TecDocApi.Application.Services;

namespace TecDocApi.API.Controllers;

/// <summary>
/// API для работы с поставщиками TecDoc
/// </summary>
[EnableCors("AllowAll")]
[ApiController]
[Route("api/v1/suppliers")]
[Tags("Suppliers")]
[Produces("application/json")]
public class TecDocSuppliersController : ControllerBase
{
    private readonly ITecDocSupplierService _supplierService;
    private readonly ILogger<TecDocSuppliersController> _logger;

    public TecDocSuppliersController(ITecDocSupplierService supplierService, ILogger<TecDocSuppliersController> logger)
    {
        _supplierService = supplierService;
        _logger = logger;
    }

    /// <summary>
    /// Поиск поставщиков по matchcode или ID
    /// </summary>
    /// <remarks>
    /// 🔍 **Поиск поставщиков:**
    /// - По matchcode (без учета пробелов и регистра)
    /// - По точному ID поставщика
    ///
    /// ### Примеры:
    /// - `matchcode=BOSCH` найдет всех поставщиков с matchcode "BOSCH"
    /// - `id=7` вернет поставщика с ID 7
    /// - Можно использовать оба параметра одновременно
    ///
    /// ### Ограничения:
    /// - Таймаут запроса: 10 секунд
    /// - Rate limit: 50 запросов за 10 секунд
    /// - Кэш: 10 минут
    /// </remarks>
    /// <param name="matchcode">Matchcode для поиска (будет нормализован: убраны пробелы, приведен к верхнему регистру)</param>
    /// <param name="id">ID поставщика для точного поиска</param>
    /// <response code="200">Поставщики найдены</response>
    /// <response code="400">Ошибка валидации параметров</response>
    /// <response code="404">Поставщики не найдены</response>
    /// <response code="429">Превышен лимит запросов</response>
    /// <response code="500">Внутренняя ошибка сервера</response>
    [HttpGet("search")]
    [EnableRateLimiting("search")]
    [ResponseCache(Duration = 600, VaryByQueryKeys = new[] { "matchcode", "id" })]
    [ProducesResponseType(typeof(SupplierSearchResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SearchSuppliers(
        [FromQuery] [StringLength(100)] string? matchcode = null, 
        [FromQuery] ushort? id = null, 
        CancellationToken cancellationToken = default)
    {
        var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        
        try
        {
            var result = await _supplierService.SearchSuppliersAsync(matchcode, id, cancellationToken);
            
            if (result is { } r && r.GetType().GetProperty("Count")?.GetValue(r) is int count && count == 0)
            {
                _logger.LogInformation("Поиск поставщиков от IP {ClientIp}: не найдено", clientIp);
                throw new NotFoundException(ErrorCodes.SUPPLIER_NOT_FOUND, "Поставщики не найдены");
            }

            _logger.LogInformation("Поиск поставщиков от IP {ClientIp}: найдено результатов", clientIp);
            return Ok(result);
        }
        catch (ApiException)
        {
            throw;
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("SECURITY: Некорректный запрос от IP {ClientIp}: {Message}", clientIp, ex.Message);
            throw new ValidationException(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при поиске поставщиков от IP {ClientIp}. Matchcode: {Matchcode}, Id: {Id}", clientIp, matchcode, id);
            throw;
        }
    }

    /// <summary>
    /// Получить поставщика по ID
    /// </summary>
    /// <remarks>
    /// Получение полной информации о поставщике по его ID, включая все детали (адреса, контакты).
    ///
    /// ### Особенности:
    /// - Возвращает полную информацию о поставщике
    /// - Включает все адреса и контактные данные
    ///
    /// ### Ограничения:
    /// - Таймаут запроса: 10 секунд
    /// - Кэш: 30 минут (поставщики меняются редко)
    /// </remarks>
    /// <param name="id">ID поставщика</param>
    /// <response code="200">Поставщик найден</response>
    /// <response code="404">Поставщик не найден</response>
    /// <response code="429">Превышен лимит запросов</response>
    /// <response code="500">Внутренняя ошибка сервера</response>
    [HttpGet("{id}")]
    [ResponseCache(Duration = 1800)]
    [ProducesResponseType(typeof(SupplierDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetSupplierById([FromRoute] ushort id, CancellationToken cancellationToken = default)
    {
        var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        
        try
        {
            var result = await _supplierService.GetSupplierByIdAsync(id, cancellationToken);
            
            if (result is { } r && r.GetType().GetProperty("Supplier")?.GetValue(r) == null)
            {
                _logger.LogInformation("Получение поставщика {Id} от IP {ClientIp}: не найдено", id, clientIp);
                throw new NotFoundException(ErrorCodes.SUPPLIER_NOT_FOUND, $"Поставщик с ID {id} не найден");
            }

            _logger.LogInformation("Получение поставщика {Id} от IP {ClientIp}: успешно", id, clientIp);
            return Ok(result);
        }
        catch (ApiException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении поставщика {Id} от IP {ClientIp}", id, clientIp);
            throw;
        }
    }
}

