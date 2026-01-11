using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using TecDocApi.Infrastructure.Data;
using TecDocApi.Infrastructure.Repositories;

namespace TecDocApi.Application.Services;

public class TecDocSupplierService : ITecDocSupplierService
{
    private readonly TecDocUnitOfWork _unitOfWork;
    private readonly IDbContextFactory<TecDocContext> _contextFactory;
    private readonly IMemoryCache _cache;
    private readonly ILogger<TecDocSupplierService> _logger;
    
    // Ограничение параллелизма для защиты БД
    private static readonly SemaphoreSlim _dbSemaphore = new(10);
    
    // Время жизни кэша
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(10);

    public TecDocSupplierService(
        TecDocUnitOfWork unitOfWork, 
        IDbContextFactory<TecDocContext> contextFactory,
        IMemoryCache cache,
        ILogger<TecDocSupplierService> logger)
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
    /// Нормализует matchcode: убирает пробелы, приводит к верхнему регистру
    /// </summary>
    private static string NormalizeMatchcode(string matchcode)
    {
        if (string.IsNullOrWhiteSpace(matchcode))
            return string.Empty;

        return new string(matchcode
            .Where(c => !char.IsWhiteSpace(c))
            .Select(c => char.ToUpperInvariant(c))
            .ToArray());
    }

    public async Task<object> SearchSuppliersAsync(string? matchcode = null, ushort? id = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var cacheKey = $"supplier_search_{matchcode ?? "null"}_{id ?? 0}";
            if (_cache.TryGetValue(cacheKey, out object? cachedResult))
            {
                return cachedResult!;
            }
            
            IQueryable<Domain.Entities.TecDoc.TdSupplier> query = _unitOfWork.Suppliers.GetAllAsNoTracking();

            List<Domain.Entities.TecDoc.TdSupplier> suppliers;

            if (id.HasValue)
            {
                suppliers = await query
                    .Where(s => s.Id == id.Value)
                    .ToListAsync(cancellationToken);
            }
            else if (!string.IsNullOrWhiteSpace(matchcode))
            {
                var normalizedMatchcode = NormalizeMatchcode(matchcode);

                var allSuppliers = await query
                    .Where(s => s.Matchcode != null)
                    .ToListAsync(cancellationToken);

                suppliers = allSuppliers
                    .Where(s => s.Matchcode != null && NormalizeMatchcode(s.Matchcode) == normalizedMatchcode)
                    .OrderBy(s => s.Id)
                    .Take(100)
                    .ToList();
            }
            else
            {
                throw new ArgumentException("Необходимо указать либо matchcode, либо id");
            }

            if (!suppliers.Any())
            {
                return new { Count = 0, Results = Array.Empty<object>() };
            }

            var supplierIds = suppliers.Select(s => s.Id).ToList();

            var supplierDetails = await RunDbAsync(async () =>
            {
                await using var db = _contextFactory.CreateDbContext();
                return await db.SupplierDetails
                    .AsNoTracking()
                    .Where(sd => supplierIds.Contains(sd.SupplierId))
                    .ToListAsync(cancellationToken);
            }, cancellationToken);

            var detailsBySupplier = supplierDetails
                .GroupBy(sd => sd.SupplierId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var result = suppliers.Select(supplier => new
            {
                Supplier = new
                {
                    Id = supplier.Id,
                    Description = supplier.Description,
                    Matchcode = supplier.Matchcode,
                    DataVersion = supplier.DataVersion,
                    NbrOfArticles = supplier.NbrOfArticles,
                    HasNewVersionArticles = supplier.HasNewVersionArticles
                },
                Details = detailsBySupplier.TryGetValue(supplier.Id, out var details)
                    ? details.Select(d => new
                    {
                        AddressType = d.AddressType,
                        AddressTypeId = d.AddressTypeId,
                        City1 = d.City1,
                        City2 = d.City2,
                        CountryCode = d.CountryCode,
                        Email = d.Email,
                        Fax = d.Fax,
                        Homepage = d.Homepage,
                        Name1 = d.Name1,
                        Name2 = d.Name2,
                        PostalCodeCity = d.PostalCodeCity,
                        PostalCodePob = d.PostalCodePob,
                        PostalCodeWholesaler = d.PostalCodeWholesaler,
                        PostalCountryCode = d.PostalCountryCode,
                        PostOfficeBox = d.PostOfficeBox,
                        Street1 = d.Street1,
                        Street2 = d.Street2,
                        Telephone = d.Telephone
                    })
                    : Enumerable.Empty<object>()
            });

            return new
            {
                Count = suppliers.Count,
                Results = result
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при поиске поставщиков. Matchcode: {Matchcode}, Id: {Id}", matchcode, id);
            throw;
        }
    }

    public async Task<object> GetSupplierByIdAsync(ushort id, CancellationToken cancellationToken = default)
    {
        try
        {
            var cacheKey = $"supplier_{id}";
            if (_cache.TryGetValue(cacheKey, out object? cachedResult))
            {
                return cachedResult!;
            }
            
            var supplier = await _unitOfWork.Suppliers.GetAllAsNoTracking()
                .Where(s => s.Id == id)
                .FirstOrDefaultAsync(cancellationToken);

            if (supplier == null)
            {
                return new { };
            }

            var details = await RunDbAsync(async () =>
            {
                await using var db = _contextFactory.CreateDbContext();
                return await db.SupplierDetails
                    .AsNoTracking()
                    .Where(sd => sd.SupplierId == id)
                    .ToListAsync(cancellationToken);
            }, cancellationToken);

            var response = new
            {
                Supplier = new
                {
                    Id = supplier.Id,
                    Description = supplier.Description,
                    Matchcode = supplier.Matchcode,
                    DataVersion = supplier.DataVersion,
                    NbrOfArticles = supplier.NbrOfArticles,
                    HasNewVersionArticles = supplier.HasNewVersionArticles
                },
                Details = details.Select(d => new
                {
                    AddressType = d.AddressType,
                    AddressTypeId = d.AddressTypeId,
                    City1 = d.City1,
                    City2 = d.City2,
                    CountryCode = d.CountryCode,
                    Email = d.Email,
                    Fax = d.Fax,
                    Homepage = d.Homepage,
                    Name1 = d.Name1,
                    Name2 = d.Name2,
                    PostalCodeCity = d.PostalCodeCity,
                    PostalCodePob = d.PostalCodePob,
                    PostalCodeWholesaler = d.PostalCodeWholesaler,
                    PostalCountryCode = d.PostalCountryCode,
                    PostOfficeBox = d.PostOfficeBox,
                    Street1 = d.Street1,
                    Street2 = d.Street2,
                    Telephone = d.Telephone
                })
            };
            
            _cache.Set(cacheKey, response, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = CacheExpiration,
                Size = 1
            });

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении поставщика {Id}", id);
            throw;
        }
    }
}

