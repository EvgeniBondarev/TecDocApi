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
        IOptions<ElasticsearchOptions> options,
        ILogger<SupplierSyncBackgroundService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _elasticsearchService = elasticsearchService;
        _logger = logger;
        _bulkSize = options.Value.SupplierBulkSize;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Служба синхронизации поставщиков запущена");

        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

        await FullSyncAsync(stoppingToken);

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
}

