using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TecDocApi.Application.Models;
using TecDocApi.Infrastructure.Repositories;

namespace TecDocApi.Application.Services;

/// <summary>
/// Фоновый сервис для синхронизации артикулов из MySQL в Elasticsearch
/// </summary>
public class ArticleSyncBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IArticleElasticsearchService _elasticsearchService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ArticleSyncBackgroundService> _logger;
    private readonly int _bulkSize;
    private readonly int _syncIntervalMinutes;

    public ArticleSyncBackgroundService(
        IServiceScopeFactory serviceScopeFactory,
        IArticleElasticsearchService elasticsearchService,
        IConfiguration configuration,
        ILogger<ArticleSyncBackgroundService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _elasticsearchService = elasticsearchService;
        _configuration = configuration;
        _logger = logger;
        _bulkSize = configuration.GetValue("Elasticsearch:BulkSize", 1000);
        _syncIntervalMinutes = configuration.GetValue("Elasticsearch:SyncIntervalMinutes", 5);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Служба синхронизации артикулов запущена");

        // Ждем немного перед первой синхронизацией, чтобы Elasticsearch успел запуститься
        await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);

        await FullSyncAsync(stoppingToken);

        // while (!stoppingToken.IsCancellationRequested)
        // {
        //     try
        //     {
        //         await IncrementalSyncAsync(stoppingToken);
        //         await Task.Delay(TimeSpan.FromMinutes(_syncIntervalMinutes), stoppingToken);
        //     }
        //     catch (OperationCanceledException)
        //     {
        //         break;
        //     }
        //     catch (Exception ex)
        //     {
        //         _logger.LogError(ex, "Ошибка в фоновой службе синхронизации");
        //         await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        //     }
        // }

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

            // Если уже проиндексировано более 90%, пропускаем полную синхронизацию
            if (indexedCount > 0 && indexedCount >= totalCount * 0.9)
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

    private async Task IncrementalSyncAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<TecDocUnitOfWork>();
        
        try
        {
            _logger.LogDebug("Начало инкрементальной синхронизации артикулов");

            // Получаем дату последней индексации из конфигурации или используем текущую дату минус час
            // var lastSyncDate = _configuration.GetValue<DateTime?>("Elasticsearch:LastSyncDate") 
            //     ?? DateTime.UtcNow.AddHours(-1);

            // Для инкрементальной синхронизации берем все артикулы, которые были изменены недавно
            // В реальном проекте здесь должна быть проверка по полю LastModified или аналогичному
            // Пока синхронизируем все артикулы, которые еще не проиндексированы
            var indexedCount = await _elasticsearchService.GetTotalCountAsync();
            var totalCount = await unitOfWork.Articles.GetAllAsNoTracking().CountAsync(cancellationToken);

            if (indexedCount >= totalCount)
            {
                _logger.LogDebug("Все артикулы уже проиндексированы");
                return;
            }

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
                .Skip((int)indexedCount)
                .Take(_bulkSize)
                .ToListAsync(cancellationToken);

            if (!articles.Any())
            {
                _logger.LogDebug("Нет новых артикулов для синхронизации");
                return;
            }

            _logger.LogInformation("Найдено {Count} новых артикулов для синхронизации", articles.Count);

            await _elasticsearchService.BulkIndexArticlesAsync(articles);

            _logger.LogInformation("Инкрементальная синхронизация артикулов завершена");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при инкрементальной синхронизации артикулов");
        }
    }
}

