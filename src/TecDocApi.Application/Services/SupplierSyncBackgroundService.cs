using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TecDocApi.Application.Models;
using TecDocApi.Infrastructure.Repositories;

namespace TecDocApi.Application.Services;

/// <summary>
/// Фоновый сервис для синхронизации поставщиков из MySQL в Elasticsearch
/// </summary>
public class SupplierSyncBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ISupplierElasticsearchService _elasticsearchService;
    private readonly ILogger<SupplierSyncBackgroundService> _logger;
    private readonly int _bulkSize;

    public SupplierSyncBackgroundService(
        IServiceScopeFactory serviceScopeFactory,
        ISupplierElasticsearchService elasticsearchService,
        IConfiguration configuration,
        ILogger<SupplierSyncBackgroundService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _elasticsearchService = elasticsearchService;
        _logger = logger;
        _bulkSize = configuration.GetValue("Elasticsearch:SupplierBulkSize", 500);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Служба синхронизации поставщиков запущена");

        // Ждем немного перед первой синхронизацией, чтобы Elasticsearch успел запуститься
        await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);

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
        //         _logger.LogError(ex, "Ошибка в фоновой службе синхронизации поставщиков");
        //         await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        //     }
        // }

        _logger.LogInformation("Служба синхронизации поставщиков остановлена");
    }

    private async Task FullSyncAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<TecDocUnitOfWork>();
        
        try
        {
            _logger.LogInformation("Начало полной синхронизации поставщиков");

            await _elasticsearchService.CreateIndexAsync();

            var totalCount = await unitOfWork.Suppliers.GetAllAsNoTracking().CountAsync(cancellationToken);
            _logger.LogInformation("Всего поставщиков в MySQL: {TotalCount}", totalCount);

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

                var suppliers = await unitOfWork.Suppliers
                    .GetAllAsNoTracking()
                    .OrderBy(s => s.Id)
                    .Skip(offset)
                    .Take(_bulkSize)
                    .Select(s => new SupplierDocument
                    {
                        SupplierId = s.Id,
                        Description = s.Description ?? string.Empty,
                        Matchcode = s.Matchcode ?? string.Empty,
                        DataVersion = s.DataVersion,
                        NbrOfArticles = s.NbrOfArticles,
                        HasNewVersionArticles = s.HasNewVersionArticles,
                        LastModified = DateTime.UtcNow
                    })
                    .ToListAsync(cancellationToken);

                if (!suppliers.Any())
                    break;

                await _elasticsearchService.BulkIndexSuppliersAsync(suppliers);
                
                offset += _bulkSize;
                _logger.LogInformation("Синхронизировано {Current} из {Total} поставщиков", 
                    Math.Min(offset, totalCount), totalCount);
            }

            _logger.LogInformation("Полная синхронизация поставщиков завершена");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при полной синхронизации поставщиков");
        }
    }

    private async Task IncrementalSyncAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<TecDocUnitOfWork>();
        
        try
        {
            _logger.LogDebug("Начало инкрементальной синхронизации поставщиков");

            var indexedCount = await _elasticsearchService.GetTotalCountAsync();
            var totalCount = await unitOfWork.Suppliers.GetAllAsNoTracking().CountAsync(cancellationToken);

            if (indexedCount >= totalCount)
            {
                _logger.LogDebug("Все поставщики уже проиндексированы");
                return;
            }

            var suppliers = await unitOfWork.Suppliers
                .GetAllAsNoTracking()
                .OrderBy(s => s.Id)
                .Skip((int)indexedCount)
                .Take(_bulkSize)
                .Select(s => new SupplierDocument
                {
                    SupplierId = s.Id,
                    Description = s.Description ?? string.Empty,
                    Matchcode = s.Matchcode ?? string.Empty,
                    DataVersion = s.DataVersion,
                    NbrOfArticles = s.NbrOfArticles,
                    HasNewVersionArticles = s.HasNewVersionArticles,
                    LastModified = DateTime.UtcNow
                })
                .ToListAsync(cancellationToken);

            if (!suppliers.Any())
            {
                _logger.LogDebug("Нет новых поставщиков для синхронизации");
                return;
            }

            _logger.LogInformation("Найдено {Count} новых поставщиков для синхронизации", suppliers.Count);

            await _elasticsearchService.BulkIndexSuppliersAsync(suppliers);

            _logger.LogInformation("Инкрементальная синхронизация поставщиков завершена");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при инкрементальной синхронизации поставщиков");
        }
    }
}

