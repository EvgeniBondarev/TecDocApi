using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TecDocApi.Application.Models;
using TecDocApi.Application.Options;
using TecDocApi.Infrastructure.Repositories;

namespace TecDocApi.Application.Services;

/// <summary>
/// Фоновый сервис для синхронизации артикулов из MySQL в Elasticsearch
/// </summary>
public class ArticleSyncBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IArticleElasticsearchService _elasticsearchService;
    private readonly ILogger<ArticleSyncBackgroundService> _logger;
    private readonly int _bulkSize;
    
    public ArticleSyncBackgroundService(
        IServiceScopeFactory serviceScopeFactory,
        IArticleElasticsearchService elasticsearchService,
        IOptions<ElasticsearchOptions> options,
        ILogger<ArticleSyncBackgroundService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _elasticsearchService = elasticsearchService;
        _logger = logger;
        _bulkSize = options.Value.BulkSize;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Служба синхронизации артикулов запущена");

        // Ждем немного перед первой синхронизацией, чтобы Elasticsearch успел запуститься
        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

        await FullSyncAsync(stoppingToken);

        _logger.LogInformation("Служба синхронизации артикулов остановлена");
    }

    private async Task FullSyncAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<TecDocUnitOfWork>();
        
        try
        {
            _logger.LogInformation("Начало полной синхронизации артикулов");

            await _elasticsearchService.CreateIndexAsync();

            var totalCount = await unitOfWork.Articles.GetAllAsNoTracking().CountAsync(cancellationToken);
            _logger.LogInformation("Всего артикулов в MySQL: {TotalCount}", totalCount);

            var indexedCount = await _elasticsearchService.GetTotalCountAsync();
            _logger.LogInformation("Уже проиндексировано в Elasticsearch: {IndexedCount}", indexedCount);

            if (indexedCount == totalCount)
            {
                _logger.LogInformation("Пропускаем полную синхронизацию, данные актуальны");
                return;
            }

            // Пересоздаем индекс для полной синхронизации
            await _elasticsearchService.DeleteIndexAsync();
            await _elasticsearchService.CreateIndexAsync();

            var offset = 0;

            while (offset < totalCount)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                var articles = await (from a in unitOfWork.Articles.GetAllAsNoTracking()
                                     join s in unitOfWork.Suppliers.GetAllAsNoTracking() 
                                     on a.SupplierId equals s.Id into supplierGroup
                                     from s in supplierGroup.DefaultIfEmpty()
                                     orderby a.SupplierId, a.DataSupplierArticleNumber
                                     select new ArticleDocument
                                     {
                                         SupplierId = a.SupplierId,
                                         DataSupplierArticleNumber = a.DataSupplierArticleNumber,
                                         FoundString = a.FoundString,
                                         NormalizedDescription = a.NormalizedDescription,
                                         Description = a.Description,
                                         ArticleStateDisplayValue = a.ArticleStateDisplayValue,
                                         QuantityPerPackingUnit = a.QuantityPerPackingUnit,
                                         SupplierDescription = s != null ? s.Description ?? string.Empty : string.Empty,
                                         SupplierMatchcode = s != null ? s.Matchcode ?? string.Empty : string.Empty,
                                         LastModified = DateTime.UtcNow
                                     })
                    .Skip(offset)
                    .Take(_bulkSize)
                    .ToListAsync(cancellationToken);

                if (!articles.Any())
                    break;

                await _elasticsearchService.BulkIndexArticlesAsync(articles);

                offset += articles.Count;

                _logger.LogInformation("Синхронизировано {Current} из {Total} артикулов", 
                    Math.Min(offset, totalCount), totalCount);
            }

            _logger.LogInformation("Полная синхронизация артикулов завершена");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при полной синхронизации артикулов");
        }
    }
}

