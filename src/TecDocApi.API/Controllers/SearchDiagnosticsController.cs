using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TecDocApi.API.Models;
using TecDocApi.Application.Services;
using TecDocApi.Infrastructure.Repositories;

namespace TecDocApi.API.Controllers;

[EnableCors("AllowAll")]
[ApiController]
[Route("api/search-diagnostics")]
[Produces("application/json")]
public class SearchDiagnosticsController : ControllerBase
{
    private readonly TecDocUnitOfWork _unitOfWork;
    private readonly IArticleElasticsearchService _articleElasticsearchService;
    private readonly ISupplierElasticsearchService _supplierElasticsearchService;
    private readonly ILogger<SearchDiagnosticsController> _logger;

    public SearchDiagnosticsController(
        TecDocUnitOfWork unitOfWork,
        IArticleElasticsearchService articleElasticsearchService,
        ISupplierElasticsearchService supplierElasticsearchService,
        ILogger<SearchDiagnosticsController> logger)
    {
        _unitOfWork = unitOfWork;
        _articleElasticsearchService = articleElasticsearchService;
        _supplierElasticsearchService = supplierElasticsearchService;
        _logger = logger;
    }

    [HttpGet("index-status")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetIndexStatus(CancellationToken cancellationToken)
    {
        try
        {
            var articleSourceTotal = await _unitOfWork.Articles.GetAllAsNoTracking().CountAsync(cancellationToken);
            var supplierSourceTotal = await _unitOfWork.Suppliers.GetAllAsNoTracking().CountAsync(cancellationToken);
            var articleIndexedDocuments = await _articleElasticsearchService.GetTotalCountAsync();
            var supplierIndexedDocuments = await _supplierElasticsearchService.GetTotalCountAsync();
            var articleIndexExists = await _articleElasticsearchService.IndexExistsAsync();
            var supplierIndexExists = await _supplierElasticsearchService.IndexExistsAsync();

            return Ok(new
            {
                timestamp = DateTime.UtcNow,
                articles = BuildStatusPayload(articleSourceTotal, articleIndexedDocuments, articleIndexExists),
                suppliers = BuildStatusPayload(supplierSourceTotal, supplierIndexedDocuments, supplierIndexExists)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении статуса индексации");
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
            {
                Code = ErrorCodes.INTERNAL_SERVER_ERROR,
                Message = "Не удалось получить статус индексации"
            });
        }
    }

    private static object BuildStatusPayload(long sourceTotal, long indexedDocuments, bool indexExists)
    {
        var completion = sourceTotal <= 0 ? 0 : Math.Round((double)indexedDocuments / sourceTotal * 100, 2);
        return new
        {
            indexExists,
            sourceTotal,
            indexedDocuments,
            remainingDocuments = Math.Max(0, sourceTotal - indexedDocuments),
            completionPercent = Math.Min(100, completion),
            isFullyIndexed = sourceTotal > 0 && indexedDocuments >= sourceTotal
        };
    }
}