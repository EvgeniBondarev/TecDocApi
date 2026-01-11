using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using TecDocApi.Domain.Entities.TecDoc;
using TecDocApi.Infrastructure.Data;
using TecDocApi.Infrastructure.Repositories;

namespace TecDocApi.Application.Services;

internal class LinkageInfo
{
    public ushort SupplierId { get; set; }
    public string DataSupplierArticleNumber { get; set; } = string.Empty;
    public string LinkageTypeId { get; set; } = string.Empty;
    public uint LinkageId { get; set; }
}

public class TecDocArticleService : ITecDocArticleService
{
    private readonly TecDocUnitOfWork _unitOfWork;
    private readonly IDbContextFactory<TecDocContext> _contextFactory;
    private readonly IMemoryCache _cache;
    private readonly ILogger<TecDocArticleService> _logger;
    
    private static readonly SemaphoreSlim _dbSemaphore = new(10);
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(5);

    public TecDocArticleService(
        TecDocUnitOfWork unitOfWork, 
        IDbContextFactory<TecDocContext> contextFactory,
        IMemoryCache cache,
        ILogger<TecDocArticleService> logger)
    {
        _unitOfWork = unitOfWork;
        _contextFactory = contextFactory;
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// Выполняет запрос к БД с ограничением параллелизма
    /// </summary>
    private async Task<T> RunDbAsync<T>(Func<Task<T>> action, CancellationToken cancellationToken = default)
    {
        await _dbSemaphore.WaitAsync(cancellationToken);
        try
        {
            return await action();
        }
        finally
        {
            _dbSemaphore.Release();
        }
    }

    /// <summary>
    /// Нормализует артикул: убирает пробелы, дефисы и другие символы, оставляет только буквы и цифры, приводит к верхнему регистру
    /// </summary>
    private static string NormalizeArticleNumber(string articleNumber)
    {
        if (string.IsNullOrWhiteSpace(articleNumber))
            return string.Empty;

        return new string(articleNumber
            .Where(c => char.IsLetterOrDigit(c))
            .Select(c => char.ToUpperInvariant(c))
            .ToArray());
    }

    public async Task<object> SearchByArticleAsync(string articleNumber, ushort? supplierId = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(articleNumber))
        {
            throw new ArgumentException("Артикул не может быть пустым", nameof(articleNumber));
        }

        try
        {
            var normalizedArticleNumber = NormalizeArticleNumber(articleNumber);
            
            var cacheKey = $"article_search_{normalizedArticleNumber}_{supplierId ?? 0}";
            if (_cache.TryGetValue(cacheKey, out object? cachedResult))
            {
                _logger.LogInformation("Кэш HIT: ключ {CacheKey}", cacheKey);
                return cachedResult!;
            }
            
            _logger.LogInformation("Кэш MISS: ключ {CacheKey}, выполняется запрос к БД", cacheKey);

            var articlesQuery = _unitOfWork.Articles.GetAllAsNoTracking()
                .Where(a => a.FoundString == normalizedArticleNumber);

            if (supplierId.HasValue)
            {
                articlesQuery = articlesQuery.Where(a => a.SupplierId == supplierId.Value);
            }

            var articles = await articlesQuery
                .OrderBy(a => a.SupplierId)
                .ThenBy(a => a.DataSupplierArticleNumber)
                .Take(50)
                .Select(a => new
                {
                    Article = a,
                    SupplierId = a.SupplierId
                })
                .ToListAsync(cancellationToken);

            if (!articles.Any())
            {
                var emptyResult = new { Count = 0, Results = Array.Empty<object>() };
                _cache.Set(cacheKey, emptyResult, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = CacheExpiration,
                    Size = 1
                });
                _logger.LogInformation("Кэш SET: ключ {CacheKey}, пустой результат (Count=0)", cacheKey);
                return emptyResult;
            }

            var supplierIds = articles.Select(a => a.SupplierId).Distinct().ToList();
            var suppliersList = supplierIds.Any()
                ? await _unitOfWork.Suppliers.GetAllAsNoTracking()
                    .Where(s => supplierIds.Contains(s.Id))
                    .ToListAsync(cancellationToken)
                : new List<Domain.Entities.TecDoc.TdSupplier>();
            
            var suppliers = suppliersList
                .GroupBy(s => s.Id)
                .ToDictionary(g => g.Key, g => g.First());

            var articleKeys = articles.Select(a => (a.Article.SupplierId, a.Article.DataSupplierArticleNumber)).ToList();

            var crossesTask = LoadCrossesAsync(articleKeys, cancellationToken);
            var oeNumbersTask = LoadOeNumbersAsync(articleKeys, cancellationToken);
            var attributesTask = LoadAttributesAsync(articleKeys, cancellationToken);
            var imagesTask = LoadImagesAsync(articleKeys, cancellationToken);
            var linkagesTask = LoadLinkagesAsync(articleKeys, cancellationToken);
            var eanCodesTask = LoadEanCodesAsync(articleKeys, cancellationToken);
            var informationTask = LoadInformationAsync(articleKeys, cancellationToken);
            var accessoriesTask = LoadAccessoriesAsync(articleKeys, cancellationToken);
            var newNumbersTask = LoadNewNumbersAsync(articleKeys, cancellationToken);

            await Task.WhenAll(
                crossesTask, oeNumbersTask, attributesTask, imagesTask, linkagesTask,
                eanCodesTask, informationTask, accessoriesTask, newNumbersTask);

            var crosses = await crossesTask;
            var oeNumbers = await oeNumbersTask;
            var attributes = await attributesTask;
            var images = await imagesTask;
            var linkages = await linkagesTask;
            var eanCodes = await eanCodesTask;
            var information = await informationTask;
            var accessories = await accessoriesTask;
            var newNumbers = await newNumbersTask;

            var result = articles.Select(a => new
            {
                Article = new
                {
                    SupplierId = a.Article.SupplierId,
                    DataSupplierArticleNumber = a.Article.DataSupplierArticleNumber,
                    FoundString = a.Article.FoundString,
                    NormalizedDescription = a.Article.NormalizedDescription,
                    Description = a.Article.Description,
                    ArticleStateDisplayValue = a.Article.ArticleStateDisplayValue,
                    QuantityPerPackingUnit = a.Article.QuantityPerPackingUnit,
                    Flags = new
                    {
                        FlagAccessory = a.Article.FlagAccessory,
                        FlagMaterialCertification = a.Article.FlagMaterialCertification,
                        FlagRemanufactured = a.Article.FlagRemanufactured,
                        FlagSelfServicePacking = a.Article.FlagSelfServicePacking,
                        HasAxle = a.Article.HasAxle,
                        HasCommercialVehicle = a.Article.HasCommercialVehicle,
                        HasEngine = a.Article.HasEngine,
                        HasLinkItems = a.Article.HasLinkItems,
                        HasMotorbike = a.Article.HasMotorbike,
                        HasPassengerCar = a.Article.HasPassengerCar,
                        IsValid = a.Article.IsValid
                    }
                },
                Supplier = suppliers.TryGetValue(a.Article.SupplierId, out var supplier) ? new
                {
                    Id = supplier.Id,
                    Description = supplier.Description,
                    Matchcode = supplier.Matchcode,
                    DataVersion = supplier.DataVersion,
                    NbrOfArticles = supplier.NbrOfArticles
                } : null,
                Crosses = crosses.TryGetValue((a.Article.SupplierId, a.Article.DataSupplierArticleNumber), out var articleCrosses)
                    ? articleCrosses : Enumerable.Empty<object>(),
                OeNumbers = oeNumbers.TryGetValue((a.Article.SupplierId, a.Article.DataSupplierArticleNumber), out var articleOeNumbers)
                    ? articleOeNumbers : Enumerable.Empty<object>(),
                Attributes = attributes.TryGetValue((a.Article.SupplierId, a.Article.DataSupplierArticleNumber), out var articleAttributes)
                    ? articleAttributes : Enumerable.Empty<object>(),
                Images = images.TryGetValue((a.Article.SupplierId, a.Article.DataSupplierArticleNumber), out var articleImages)
                    ? articleImages : Enumerable.Empty<object>(),
                Linkages = linkages.TryGetValue((a.Article.SupplierId, a.Article.DataSupplierArticleNumber), out var articleLinkages)
                    ? articleLinkages : Enumerable.Empty<object>(),
                EanCodes = eanCodes.TryGetValue((a.Article.SupplierId, a.Article.DataSupplierArticleNumber), out var articleEanCodes)
                    ? articleEanCodes : Enumerable.Empty<object>(),
                Information = information.TryGetValue((a.Article.SupplierId, a.Article.DataSupplierArticleNumber), out var articleInformation)
                    ? articleInformation : Enumerable.Empty<object>(),
                Accessories = accessories.TryGetValue((a.Article.SupplierId, a.Article.DataSupplierArticleNumber), out var articleAccessories)
                    ? articleAccessories : Enumerable.Empty<object>(),
                NewNumbers = newNumbers.TryGetValue((a.Article.SupplierId, a.Article.DataSupplierArticleNumber), out var articleNewNumbers)
                    ? articleNewNumbers : Enumerable.Empty<object>()
            });

            var response = new
            {
                Count = articles.Count,
                Results = result
            };
            
            // Сохраняем в кэш
            _cache.Set(cacheKey, response, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = CacheExpiration,
                Size = 1
            });
            
            _logger.LogInformation("Кэш SET: ключ {CacheKey}, Count={Count}", cacheKey, response.Count);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при поиске артикула {ArticleNumber}", articleNumber);
            throw;
        }
    }

    public async Task<object> GetByExactMatchAsync(ushort supplierId, string articleNumber, CancellationToken cancellationToken = default)
    {
        try
        {
            var normalizedArticleNumber = NormalizeArticleNumber(articleNumber);
            
            var cacheKey = $"article_exact_{supplierId}_{normalizedArticleNumber}";
            if (_cache.TryGetValue(cacheKey, out object? cachedResult))
            {
                return cachedResult!;
            }

            var article = await _unitOfWork.Articles.GetAllAsNoTracking()
                .Where(a => a.SupplierId == supplierId && a.FoundString == normalizedArticleNumber)
                .Select(a => new
                {
                    a.SupplierId,
                    a.DataSupplierArticleNumber,
                    a.FoundString,
                    a.NormalizedDescription,
                    a.Description,
                    a.ArticleStateDisplayValue,
                    a.QuantityPerPackingUnit,
                    a.FlagAccessory,
                    a.FlagMaterialCertification,
                    a.FlagRemanufactured,
                    a.FlagSelfServicePacking,
                    a.HasAxle,
                    a.HasCommercialVehicle,
                    a.HasEngine,
                    a.HasLinkItems,
                    a.HasMotorbike,
                    a.HasPassengerCar,
                    a.IsValid
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (article == null)
            {
                return new { };
            }

            var articleKey = (supplierId, article.DataSupplierArticleNumber);
            var articleKeys = new List<(ushort SupplierId, string DataSupplierArticleNumber)> { articleKey };

            // Параллельная загрузка всех связанных данных
            var supplierTask = RunDbAsync(async () =>
            {
                await using var db = _contextFactory.CreateDbContext();
                return await db.Suppliers
                    .AsNoTracking()
                    .Where(s => s.Id == supplierId)
                    .Select(s => new { s.Id, s.Description, s.Matchcode })
                    .FirstOrDefaultAsync(cancellationToken);
            }, cancellationToken);

            var crossesTask = LoadCrossesAsync(articleKeys, cancellationToken);
            var oeNumbersTask = LoadOeNumbersAsync(articleKeys, cancellationToken);
            var attributesTask = LoadAttributesAsync(articleKeys, cancellationToken);
            var imagesTask = LoadImagesAsync(articleKeys, cancellationToken);
            var linkagesTask = LoadLinkagesAsync(articleKeys, cancellationToken);
            var eanCodesTask = LoadEanCodesAsync(articleKeys, cancellationToken);
            var informationTask = LoadInformationAsync(articleKeys, cancellationToken);
            var accessoriesTask = LoadAccessoriesAsync(articleKeys, cancellationToken);
            var newNumbersTask = LoadNewNumbersAsync(articleKeys, cancellationToken);

            await Task.WhenAll(
                supplierTask, crossesTask, oeNumbersTask, attributesTask, imagesTask,
                linkagesTask, eanCodesTask, informationTask, accessoriesTask, newNumbersTask);

            var supplier = await supplierTask;
            var crosses = await crossesTask;
            var oeNumbers = await oeNumbersTask;
            var attributes = await attributesTask;
            var images = await imagesTask;
            var linkages = await linkagesTask;
            var eanCodes = await eanCodesTask;
            var information = await informationTask;
            var accessories = await accessoriesTask;
            var newNumbers = await newNumbersTask;

            var response = new
            {
                Article = new
                {
                    article.SupplierId,
                    article.DataSupplierArticleNumber,
                    article.FoundString,
                    article.NormalizedDescription,
                    article.Description,
                    article.ArticleStateDisplayValue,
                    article.QuantityPerPackingUnit,
                    Flags = new
                    {
                        article.FlagAccessory,
                        article.FlagMaterialCertification,
                        article.FlagRemanufactured,
                        article.FlagSelfServicePacking,
                        article.HasAxle,
                        article.HasCommercialVehicle,
                        article.HasEngine,
                        article.HasLinkItems,
                        article.HasMotorbike,
                        article.HasPassengerCar,
                        article.IsValid
                    }
                },
                Supplier = supplier != null ? new
                {
                    supplier.Id,
                    supplier.Description,
                    supplier.Matchcode
                } : null,
                Crosses = crosses.TryGetValue(articleKey, out var crossList) ? crossList : Enumerable.Empty<object>(),
                OeNumbers = oeNumbers.TryGetValue(articleKey, out var oeList) ? oeList : Enumerable.Empty<object>(),
                Attributes = attributes.TryGetValue(articleKey, out var attrList) ? attrList : Enumerable.Empty<object>(),
                Images = images.TryGetValue(articleKey, out var imgList) ? imgList : Enumerable.Empty<object>(),
                Linkages = linkages.TryGetValue(articleKey, out var linkList) ? linkList : Enumerable.Empty<object>(),
                EanCodes = eanCodes.TryGetValue(articleKey, out var eanList) ? eanList : Enumerable.Empty<object>(),
                Information = information.TryGetValue(articleKey, out var infList) ? infList : Enumerable.Empty<object>(),
                Accessories = accessories.TryGetValue(articleKey, out var accList) ? accList : Enumerable.Empty<object>(),
                NewNumbers = newNumbers.TryGetValue(articleKey, out var nnList) ? nnList : Enumerable.Empty<object>()
            };
            
            // Сохраняем в кэш
            _cache.Set(cacheKey, response, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = CacheExpiration,
                Size = 1
            });
            
            _logger.LogInformation("Кэш SET: ключ {CacheKey}", cacheKey);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении артикула {SupplierId}/{ArticleNumber}", supplierId, articleNumber);
            throw;
        }
    }

    private async Task<Dictionary<(ushort SupplierId, string ArticleNumber), List<object>>> LoadCrossesAsync(
        List<(ushort SupplierId, string DataSupplierArticleNumber)> articleKeys, CancellationToken cancellationToken)
    {
        return await RunDbAsync(async () =>
        {
            await using var db = _contextFactory.CreateDbContext();
            
            var supplierIds = articleKeys.Select(k => k.SupplierId).Distinct().ToList();
            var articleNumbers = articleKeys.Select(k => k.DataSupplierArticleNumber).Distinct().ToList();

            var crosses = await db.ArticleCrosses
                .AsNoTracking()
                .Where(c => supplierIds.Contains(c.SupplierId) && articleNumbers.Contains(c.PartsDataSupplierArticleNumber))
                .Select(c => new { c.SupplierId, c.PartsDataSupplierArticleNumber, c.ManufacturerId, c.OENbr })
                .ToListAsync(cancellationToken);

            var manufacturerIds = crosses.Select(c => c.ManufacturerId).Distinct().ToList();
            
            var manufacturersDict = manufacturerIds.Any()
                ? (await db.Manufacturers
                    .AsNoTracking()
                    .Where(m => manufacturerIds.Contains(m.Id))
                    .Select(m => new { m.Id, m.Description })
                    .ToListAsync(cancellationToken))
                    .GroupBy(m => m.Id)
                    .ToDictionary(g => g.Key, g => new Domain.Entities.TecDoc.TdManufacturer { Id = g.Key, Description = g.First().Description })
                : new Dictionary<uint, Domain.Entities.TecDoc.TdManufacturer>();

            var result = new Dictionary<(ushort, string), List<object>>();
            foreach (var cross in crosses)
            {
                var key = (cross.SupplierId, cross.PartsDataSupplierArticleNumber);
                if (!result.ContainsKey(key))
                    result[key] = new List<object>();

                result[key].Add(new
                {
                    ManufacturerId = cross.ManufacturerId,
                    OENbr = cross.OENbr,
                    Manufacturer = manufacturersDict.TryGetValue(cross.ManufacturerId, out var m) 
                        ? new { m.Id, m.Description } 
                        : null
                });
            }
            return result;
        }, cancellationToken);
    }

    private async Task<Dictionary<(ushort SupplierId, string ArticleNumber), List<object>>> LoadOeNumbersAsync(
        List<(ushort SupplierId, string DataSupplierArticleNumber)> articleKeys, CancellationToken cancellationToken)
    {
        return await RunDbAsync(async () =>
        {
            await using var db = _contextFactory.CreateDbContext();
            
            var supplierIds = articleKeys.Select(k => k.SupplierId).Distinct().ToList();
            var articleNumbers = articleKeys.Select(k => k.DataSupplierArticleNumber).Distinct().ToList();

            var oeNumbers = await db.ArticleOes
                .AsNoTracking()
                .Where(oe => supplierIds.Contains(oe.SupplierId) && articleNumbers.Contains(oe.DataSupplierArticleNumber))
                .Select(oe => new { oe.SupplierId, oe.DataSupplierArticleNumber, oe.OENbr, oe.IsAdditive })
                .ToListAsync(cancellationToken);

            var result = new Dictionary<(ushort, string), List<object>>();
            foreach (var oe in oeNumbers)
            {
                var key = (oe.SupplierId, oe.DataSupplierArticleNumber);
                if (!result.ContainsKey(key))
                    result[key] = new List<object>();

                result[key].Add(new { oe.OENbr, oe.IsAdditive });
            }
            return result;
        }, cancellationToken);
    }

    private async Task<Dictionary<(ushort SupplierId, string ArticleNumber), List<object>>> LoadAttributesAsync(
        List<(ushort SupplierId, string DataSupplierArticleNumber)> articleKeys, CancellationToken cancellationToken)
    {
        return await RunDbAsync(async () =>
        {
            await using var db = _contextFactory.CreateDbContext();
            
            var supplierIds = articleKeys.Select(k => k.SupplierId).Distinct().ToList();
            var articleNumbers = articleKeys.Select(k => k.DataSupplierArticleNumber).Distinct().ToList();

            var attributes = await db.ArticleAttributes
                .AsNoTracking()
                .Where(attr => supplierIds.Contains(attr.SupplierId) && articleNumbers.Contains(attr.DataSupplierArticleNumber))
                .Select(attr => new { attr.SupplierId, attr.DataSupplierArticleNumber, attr.Id, attr.Description, attr.DisplayTitle, attr.DisplayValue })
                .ToListAsync(cancellationToken);

            var result = new Dictionary<(ushort, string), List<object>>();
            foreach (var attr in attributes)
            {
                var key = (attr.SupplierId, attr.DataSupplierArticleNumber);
                if (!result.ContainsKey(key))
                    result[key] = new List<object>();

                result[key].Add(new { attr.Id, attr.Description, attr.DisplayTitle, attr.DisplayValue });
            }
            return result;
        }, cancellationToken);
    }

    private async Task<Dictionary<(ushort SupplierId, string ArticleNumber), List<object>>> LoadImagesAsync(
        List<(ushort SupplierId, string DataSupplierArticleNumber)> articleKeys, CancellationToken cancellationToken)
    {
        return await RunDbAsync(async () =>
        {
            await using var db = _contextFactory.CreateDbContext();
            
            var supplierIds = articleKeys.Select(k => k.SupplierId).Distinct().ToList();
            var articleNumbers = articleKeys.Select(k => k.DataSupplierArticleNumber).Distinct().ToList();

            var images = await db.ArticleImages
                .AsNoTracking()
                .Where(img => supplierIds.Contains(img.SupplierId) && articleNumbers.Contains(img.DataSupplierArticleNumber))
                .Select(img => new { img.SupplierId, img.DataSupplierArticleNumber, img.PictureName, img.Description, img.AdditionalDescription, img.DocumentName, img.DocumentType, img.ShowImmediately })
                .ToListAsync(cancellationToken);

            var result = new Dictionary<(ushort, string), List<object>>();
            foreach (var img in images)
            {
                var key = (img.SupplierId, img.DataSupplierArticleNumber);
                if (!result.ContainsKey(key))
                    result[key] = new List<object>();

                result[key].Add(new { img.PictureName, img.Description, img.AdditionalDescription, img.DocumentName, img.DocumentType, img.ShowImmediately });
            }
            return result;
        }, cancellationToken);
    }

    private async Task<Dictionary<(ushort SupplierId, string ArticleNumber), List<object>>> LoadLinkagesAsync(
        List<(ushort SupplierId, string DataSupplierArticleNumber)> articleKeys, CancellationToken cancellationToken)
    {
        return await RunDbAsync(async () =>
        {
            await using var db = _contextFactory.CreateDbContext();
            
            var supplierIds = articleKeys.Select(k => k.SupplierId).Distinct().ToList();
            var articleNumbers = articleKeys.Select(k => k.DataSupplierArticleNumber).Distinct().ToList();

            var linkages = await db.ArticleLis
                .AsNoTracking()
                .Where(li => supplierIds.Contains(li.SupplierId) && articleNumbers.Contains(li.DataSupplierArticleNumber))
                .Select(li => new LinkageInfo 
                { 
                    SupplierId = li.SupplierId, 
                    DataSupplierArticleNumber = li.DataSupplierArticleNumber, 
                    LinkageTypeId = li.LinkageTypeId, 
                    LinkageId = li.LinkageId 
                })
                .ToListAsync(cancellationToken);

            var passengerCarIds = linkages
                .Where(li => li.LinkageTypeId == "PassengerCar")
                .Select(li => li.LinkageId)
                .Distinct()
                .ToList();

            if (!passengerCarIds.Any())
            {
                var result = new Dictionary<(ushort, string), List<object>>();
                foreach (var li in linkages)
                {
                    var key = (li.SupplierId, li.DataSupplierArticleNumber);
                    if (!result.ContainsKey(key))
                        result[key] = new List<object>();
                    result[key].Add(new { LinkageTypeId = li.LinkageTypeId, LinkageId = li.LinkageId });
                }
                return result;
            }

            var passengerCarsTask = RunDbAsync(async () =>
            {
                await using var db2 = _contextFactory.CreateDbContext();
                return await db2.PassengerCars
                    .AsNoTracking()
                    .Where(pc => passengerCarIds.Contains(pc.Id))
                    .ToListAsync(cancellationToken);
            }, cancellationToken);

            var passengerCarAttributesTask = RunDbAsync(async () =>
            {
                await using var db2 = _contextFactory.CreateDbContext();
                return await db2.PassengerCarAttributes
                    .AsNoTracking()
                    .Where(pca => passengerCarIds.Contains(pca.PassengerCarId))
                    .ToListAsync(cancellationToken);
            }, cancellationToken);

            await Task.WhenAll(passengerCarsTask, passengerCarAttributesTask);

            var passengerCars = await passengerCarsTask;
            var passengerCarAttributes = await passengerCarAttributesTask;

            var modelIds = passengerCars
                .Where(pc => pc.ModelId.HasValue)
                .Select(pc => pc.ModelId!.Value)
                .Distinct()
                .ToList();

            if (!modelIds.Any())
            {
                var emptyModelsById = new Dictionary<uint, TdModel>();
                var emptyAttributesByCarId = passengerCarAttributes
                    .GroupBy(pca => pca.PassengerCarId)
                    .ToDictionary(g => g.Key, g => g.ToList());

                return BuildLinkagesResult(linkages, passengerCars, emptyModelsById, new Dictionary<uint, TdManufacturer>(), emptyAttributesByCarId);
            }

            // Параллельная загрузка Models и Manufacturers
            var modelsTask = RunDbAsync(async () =>
            {
                await using var db2 = _contextFactory.CreateDbContext();
                return await db2.Models
                    .AsNoTracking()
                    .Where(m => modelIds.Contains(m.Id))
                    .ToListAsync(cancellationToken);
            }, cancellationToken);

            var models = await modelsTask;

            var manufacturerIds = models
                .Where(m => m.ManufacturerId.HasValue)
                .Select(m => m.ManufacturerId!.Value)
                .Distinct()
                .ToList();

            var manufacturers = manufacturerIds.Any()
                ? await RunDbAsync(async () =>
                {
                    await using var db2 = _contextFactory.CreateDbContext();
                    return (await db2.Manufacturers
                        .AsNoTracking()
                        .Where(m => manufacturerIds.Contains(m.Id))
                        .Select(m => new { m.Id, m.Description })
                        .ToListAsync(cancellationToken))
                        .GroupBy(m => m.Id)
                        .ToDictionary(g => g.Key, g => new TdManufacturer { Id = g.Key, Description = g.First().Description });
                }, cancellationToken)
                : new Dictionary<uint, TdManufacturer>();

            var modelsById = models.ToDictionary(m => m.Id);
            var attributesByCarId = passengerCarAttributes
                .GroupBy(pca => pca.PassengerCarId)
                .ToDictionary(g => g.Key, g => g.ToList());

            return BuildLinkagesResult(linkages, passengerCars, modelsById, manufacturers, attributesByCarId);
        }, cancellationToken);
    }

    private Dictionary<(ushort, string), List<object>> BuildLinkagesResult(
        List<LinkageInfo> linkages,
        List<TdPassengerCar> passengerCars,
        Dictionary<uint, TdModel> modelsById,
        Dictionary<uint, TdManufacturer> manufacturers,
        Dictionary<uint, List<TdPassengerCarAttribute>> attributesByCarId)
    {
        var result = new Dictionary<(ushort, string), List<object>>();
        foreach (var li in linkages)
        {
            var key = (li.SupplierId, li.DataSupplierArticleNumber);
            if (!result.ContainsKey(key))
                result[key] = new List<object>();

            if (li.LinkageTypeId == "PassengerCar")
            {
                var passengerCar = passengerCars.FirstOrDefault(pc => pc.Id == li.LinkageId);
                if (passengerCar != null)
                {
                    var attributes = attributesByCarId.TryGetValue(passengerCar.Id, out var attrs)
                        ? attrs.Select(a => new
                        {
                            AttributeGroup = a.AttributeGroup,
                            AttributeType = a.AttributeType,
                            DisplayTitle = a.DisplayTitle,
                            DisplayValue = a.DisplayValue
                        }).Cast<object>().ToList()
                        : new List<object>();

                    var model = passengerCar.ModelId.HasValue && modelsById.TryGetValue(passengerCar.ModelId.Value, out var m)
                        ? m
                        : null;

                    var manufacturer = model?.ManufacturerId.HasValue == true && manufacturers.TryGetValue(model.ManufacturerId.Value, out var man)
                        ? man
                        : null;

                    result[key].Add(new
                    {
                        LinkageTypeId = li.LinkageTypeId,
                        LinkageId = li.LinkageId,
                        Vehicle = new
                        {
                            Id = passengerCar.Id,
                            Description = passengerCar.Description,
                            FullDescription = passengerCar.FullDescription,
                            ConstructionInterval = passengerCar.ConstructionInterval,
                            CanBeDisplayed = passengerCar.CanBeDisplayed,
                            HasLink = passengerCar.HasLink,
                            TypeFlags = new
                            {
                                IsAxle = passengerCar.IsAxle,
                                IsCommercialVehicle = passengerCar.IsCommercialVehicle,
                                IsCvManufacturerId = passengerCar.IsCvManufacturerId,
                                IsEngine = passengerCar.IsEngine,
                                IsMotorbike = passengerCar.IsMotorbike,
                                IsPassengerCar = passengerCar.IsPassengerCar,
                                IsTransporter = passengerCar.IsTransporter
                            },
                            Model = model != null ? new
                            {
                                Id = model.Id,
                                Description = model.Description,
                                FullDescription = model.FullDescription,
                                ConstructionInterval = model.ConstructionInterval,
                                Manufacturer = manufacturer != null ? new
                                {
                                    Id = manufacturer.Id,
                                    Description = manufacturer.Description
                                } : null
                            } : null,
                            Attributes = attributes
                        }
                    });
                }
                else
                {
                    result[key].Add(new { LinkageTypeId = li.LinkageTypeId, LinkageId = li.LinkageId });
                }
            }
            else
            {
                result[key].Add(new { LinkageTypeId = li.LinkageTypeId, LinkageId = li.LinkageId });
            }
        }
        return result;
    }

    private async Task<Dictionary<(ushort SupplierId, string ArticleNumber), List<object>>> LoadEanCodesAsync(
        List<(ushort SupplierId, string DataSupplierArticleNumber)> articleKeys, CancellationToken cancellationToken)
    {
        return await RunDbAsync(async () =>
        {
            await using var db = _contextFactory.CreateDbContext();
            
            var supplierIds = articleKeys.Select(k => k.SupplierId).Distinct().ToList();
            var articleNumbers = articleKeys.Select(k => k.DataSupplierArticleNumber).Distinct().ToList();

            var eanCodes = await db.ArticleEans
                .AsNoTracking()
                .Where(ean => supplierIds.Contains(ean.SupplierId) && articleNumbers.Contains(ean.DataSupplierArticleNumber))
                .Select(ean => new { ean.SupplierId, ean.DataSupplierArticleNumber, ean.Ean })
                .ToListAsync(cancellationToken);

            var result = new Dictionary<(ushort, string), List<object>>();
            foreach (var ean in eanCodes)
            {
                var key = (ean.SupplierId, ean.DataSupplierArticleNumber);
                if (!result.ContainsKey(key))
                    result[key] = new List<object>();

                result[key].Add(new { ean.Ean });
            }
            return result;
        }, cancellationToken);
    }

    private async Task<Dictionary<(ushort SupplierId, string ArticleNumber), List<object>>> LoadInformationAsync(
        List<(ushort SupplierId, string DataSupplierArticleNumber)> articleKeys, CancellationToken cancellationToken)
    {
        return await RunDbAsync(async () =>
        {
            await using var db = _contextFactory.CreateDbContext();
            
            var supplierIds = articleKeys.Select(k => k.SupplierId).Distinct().ToList();
            var articleNumbers = articleKeys.Select(k => k.DataSupplierArticleNumber).Distinct().ToList();

            var information = await db.ArticleInfs
                .AsNoTracking()
                .Where(inf => supplierIds.Contains(inf.SupplierId) && articleNumbers.Contains(inf.DataSupplierArticleNumber))
                .Select(inf => new { inf.SupplierId, inf.DataSupplierArticleNumber, inf.InformationTypeKey, inf.InformationType, inf.InformationText })
                .ToListAsync(cancellationToken);

            var result = new Dictionary<(ushort, string), List<object>>();
            foreach (var inf in information)
            {
                var key = (inf.SupplierId, inf.DataSupplierArticleNumber);
                if (!result.ContainsKey(key))
                    result[key] = new List<object>();

                result[key].Add(new { inf.InformationTypeKey, inf.InformationType, inf.InformationText });
            }
            return result;
        }, cancellationToken);
    }

    private async Task<Dictionary<(ushort SupplierId, string ArticleNumber), List<object>>> LoadAccessoriesAsync(
        List<(ushort SupplierId, string DataSupplierArticleNumber)> articleKeys, CancellationToken cancellationToken)
    {
        return await RunDbAsync(async () =>
        {
            await using var db = _contextFactory.CreateDbContext();
            
            var supplierIds = articleKeys.Select(k => k.SupplierId).Distinct().ToList();
            var articleNumbers = articleKeys.Select(k => k.DataSupplierArticleNumber).Distinct().ToList();

            var accessories = await db.ArticleAccs
                .AsNoTracking()
                .Where(acc => supplierIds.Contains(acc.SupplierId) && articleNumbers.Contains(acc.DataSupplierArticleNumber))
                .Select(acc => new { acc.SupplierId, acc.DataSupplierArticleNumber, acc.AccSupplierId, acc.AccDataSupplierArticleNumber })
                .ToListAsync(cancellationToken);

            var accSupplierIds = accessories.Select(acc => acc.AccSupplierId).Distinct().ToList();
            var accSuppliers = accSupplierIds.Any()
                ? (await db.Suppliers
                    .AsNoTracking()
                    .Where(s => accSupplierIds.Contains(s.Id))
                    .Select(s => new { s.Id, s.Description })
                    .ToListAsync(cancellationToken))
                    .GroupBy(s => s.Id)
                    .ToDictionary(g => g.Key, g => new Domain.Entities.TecDoc.TdSupplier { Id = g.Key, Description = g.First().Description })
                : new Dictionary<ushort, Domain.Entities.TecDoc.TdSupplier>();

            var result = new Dictionary<(ushort, string), List<object>>();
            foreach (var acc in accessories)
            {
                var key = (acc.SupplierId, acc.DataSupplierArticleNumber);
                if (!result.ContainsKey(key))
                    result[key] = new List<object>();

                result[key].Add(new
                {
                    acc.AccSupplierId,
                    acc.AccDataSupplierArticleNumber,
                    AccSupplier = accSuppliers.TryGetValue(acc.AccSupplierId, out var s) ? new { s.Id, s.Description } : null
                });
            }
            return result;
        }, cancellationToken);
    }

    private async Task<Dictionary<(ushort SupplierId, string ArticleNumber), List<object>>> LoadNewNumbersAsync(
        List<(ushort SupplierId, string DataSupplierArticleNumber)> articleKeys, CancellationToken cancellationToken)
    {
        return await RunDbAsync(async () =>
        {
            await using var db = _contextFactory.CreateDbContext();
            
            var supplierIds = articleKeys.Select(k => k.SupplierId).Distinct().ToList();
            var articleNumbers = articleKeys.Select(k => k.DataSupplierArticleNumber).Distinct().ToList();

            var newNumbers = await db.ArticleNns
                .AsNoTracking()
                .Where(nn => supplierIds.Contains(nn.SupplierId) && articleNumbers.Contains(nn.DataSupplierArticleNumber))
                .Select(nn => new { nn.SupplierId, nn.DataSupplierArticleNumber, nn.NewSupplierId, nn.NewDataSupplierArticleNumber })
                .ToListAsync(cancellationToken);

            var newSupplierIds = newNumbers.Select(nn => nn.NewSupplierId).Where(id => id > 0).Distinct().ToList();
            var newSuppliers = newSupplierIds.Any()
                ? (await db.Suppliers
                    .AsNoTracking()
                    .Where(s => newSupplierIds.Contains(s.Id))
                    .Select(s => new { s.Id, s.Description })
                    .ToListAsync(cancellationToken))
                    .GroupBy(s => s.Id)
                    .ToDictionary(g => g.Key, g => new Domain.Entities.TecDoc.TdSupplier { Id = g.Key, Description = g.First().Description })
                : new Dictionary<ushort, Domain.Entities.TecDoc.TdSupplier>();

            var result = new Dictionary<(ushort, string), List<object>>();
            foreach (var nn in newNumbers)
            {
                var key = (nn.SupplierId, nn.DataSupplierArticleNumber);
                if (!result.ContainsKey(key))
                    result[key] = new List<object>();

                result[key].Add(new
                {
                    NewSupplierId = nn.NewSupplierId,
                    NewDataSupplierArticleNumber = nn.NewDataSupplierArticleNumber,
                    NewSupplier = nn.NewSupplierId > 0 && newSuppliers.TryGetValue(nn.NewSupplierId, out var s)
                        ? new { s.Id, s.Description } : null
                });
            }
            return result;
        }, cancellationToken);
    }
}

