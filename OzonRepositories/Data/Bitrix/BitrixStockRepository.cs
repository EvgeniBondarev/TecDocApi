using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using Newtonsoft.Json;
using OzonDomains.Models;
using OzonDomains.Models.BitrixModels;
using OzonOrdersWeb.Areas.Studio2.ViewModels.Bitrix;
using OzonRepositories.Context;

namespace OzonRepositories.Data.Bitrix;

public class BitrixStockRepository
{
    private readonly BitrixContext _context;
    private readonly SupplierRepository _supplierRepository;

    public BitrixStockRepository(BitrixContext context,
                                 SupplierRepository supplierRepository)
    {
        _context = context;
        _supplierRepository = supplierRepository;
    }
    
    public async Task<PagedResult<RemainingStockBitrix>> GetRemainingStockAsync(RemainingStockFilter filter)
    {
        // ==== SQL-запрос плоской выборки ====
        var sql = @"
            SELECT 
                CAST(e.ID AS SIGNED) AS ProductId,
                MAX(p.VALUE) AS Article,
                e.PREVIEW_TEXT AS PreviewText,
                e.ACTIVE AS Active,
                e.TIMESTAMP_X AS TimestampX,
                c.QUANTITY AS Quantity,
                CAST(c.AVAILABLE AS CHAR) AS Available,
                supplier_enum.VALUE AS Supplier,
                price.PRICE AS Price,
                sp.STORE_ID AS StoreId,
                s.TITLE AS StoreTitle,
                sp.AMOUNT AS Amount
            FROM b_iblock_element e
            LEFT JOIN b_iblock_element_property p 
                ON e.ID = p.IBLOCK_ELEMENT_ID AND p.DESCRIPTION = 'Артикул'
            LEFT JOIN b_catalog_product c 
                ON c.ID = e.ID
            LEFT JOIN b_iblock_element_property prop_supplier 
                ON e.ID = prop_supplier.IBLOCK_ELEMENT_ID AND prop_supplier.IBLOCK_PROPERTY_ID = 2458
            LEFT JOIN b_iblock_property_enum supplier_enum 
                ON prop_supplier.VALUE = supplier_enum.ID AND supplier_enum.PROPERTY_ID = 2458
            LEFT JOIN b_catalog_price price
                ON price.PRODUCT_ID = e.ID AND price.CATALOG_GROUP_ID = 3
            LEFT JOIN b_catalog_store_product sp
                ON sp.PRODUCT_ID = e.ID
            LEFT JOIN b_catalog_store s
                ON sp.STORE_ID = s.ID
            WHERE e.IBLOCK_ID = 93
              AND sp.AMOUNT > 0
            GROUP BY e.ID, e.PREVIEW_TEXT, e.ACTIVE, e.TIMESTAMP_X, 
                     c.QUANTITY, c.AVAILABLE, supplier_enum.VALUE, price.PRICE, 
                     sp.STORE_ID, s.TITLE, sp.AMOUNT
        ";

        var flatQuery = _context.Set<RemainingStockFlat>().FromSqlRaw(sql).AsQueryable();

        // ==== Применяем фильтры ====
        if (filter.ProductId.HasValue)
            flatQuery = flatQuery.Where(x => x.ProductId == filter.ProductId.Value);

        if (!string.IsNullOrWhiteSpace(filter.Article))
            flatQuery = flatQuery.Where(x => x.Article != null && x.Article.Contains(filter.Article));

        if (!string.IsNullOrWhiteSpace(filter.PreviewText))
            flatQuery = flatQuery.Where(x => x.PreviewText != null && x.PreviewText.Contains(filter.PreviewText));

        if (!string.IsNullOrWhiteSpace(filter.Active))
            flatQuery = flatQuery.Where(x => x.Active == filter.Active);

        if (!string.IsNullOrWhiteSpace(filter.Available))
            flatQuery = flatQuery.Where(x => x.Available == filter.Available);

        if (!string.IsNullOrWhiteSpace(filter.Supplier))
            flatQuery = flatQuery.Where(x => x.Supplier != null && x.Supplier.Contains(filter.Supplier));

        if (filter.QuantityFrom.HasValue)
            flatQuery = flatQuery.Where(x => x.Quantity >= filter.QuantityFrom.Value);

        if (filter.QuantityTo.HasValue)
            flatQuery = flatQuery.Where(x => x.Quantity <= filter.QuantityTo.Value);

        if (filter.DateFrom.HasValue)
            flatQuery = flatQuery.Where(x => x.TimestampX >= filter.DateFrom.Value);

        if (filter.DateTo.HasValue)
            flatQuery = flatQuery.Where(x => x.TimestampX < filter.DateTo.Value.AddDays(1));

        if (filter.PriceValueFrom.HasValue)
            flatQuery = flatQuery.Where(x => x.Price >= (double)filter.PriceValueFrom.Value);

        if (filter.PriceValueTo.HasValue)
            flatQuery = flatQuery.Where(x => x.Price <= (double)filter.PriceValueTo.Value);

        if (filter.StoreId.HasValue)
            flatQuery = flatQuery.Where(x => x.StoreId == filter.StoreId.Value);

        if (!string.IsNullOrWhiteSpace(filter.StoreTitle))
            flatQuery = flatQuery.Where(x => x.StoreTitle.Contains(filter.StoreTitle));

        if (filter.AmountFrom.HasValue)
            flatQuery = flatQuery.Where(x => x.Amount >= filter.AmountFrom.Value);

        if (filter.AmountTo.HasValue)
            flatQuery = flatQuery.Where(x => x.Amount <= filter.AmountTo.Value);

        // ==== Получаем ID уникальных товаров для текущей страницы ====
        var productIdsPageQuery = flatQuery
            .GroupBy(f => f.ProductId)
            .Select(g => g.Key);

        // Сортировка уникальных товаров
        if (!string.IsNullOrEmpty(filter.SortColumn))
        {
            var sortProperty = GetSortPropertyFlat(filter.SortColumn);

            flatQuery = filter.SortDirection?.ToLower() == "desc"
                ? flatQuery.OrderByDescending(sortProperty)
                : flatQuery.OrderBy(sortProperty);
        }


        var totalCount = await productIdsPageQuery.CountAsync();

        var productIdsPage = await productIdsPageQuery
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        // ==== Получаем все flat записи для этих товаров ====
        var flatItemsPage = await flatQuery
            .Where(f => productIdsPage.Contains(f.ProductId))
            .ToListAsync();

        // ==== Группируем на клиенте в RemainingStockBitrix ====
        var items = flatItemsPage
            .GroupBy(f => f.ProductId)
            .Select(g => new RemainingStockBitrix
            {
                ProductId = g.Key,
                Article = g.First().Article,
                PreviewText = g.First().PreviewText,
                Active = g.First().Active,
                Available = g.First().Available,
                Supplier = g.First().Supplier,
                TimestampX = g.First().TimestampX,
                Quantity = g.First().Quantity,
                Price = g.First().Price,
                Stores = g.Select(s => new StockByStore
                {
                    StoreId = s.StoreId,
                    Title = s.StoreTitle,
                    Amount = s.Amount
                }).ToList()
            })
            .ToList();

        return new PagedResult<RemainingStockBitrix>
        {
            Items = items,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }
    
    public async Task<List<string>> GetActiveStoresAsync()
    {
        var stores = await (from sp in _context.Set<BCatalogStore>()
                join s in _context.Set<BCatalogStore>() on sp.Id equals s.Id
                where s.Active == "Y"
                select s.Title)
            .Distinct()
            .ToListAsync();

        return stores
            .Where(t => !string.IsNullOrEmpty(t))
            .OrderBy(t => t) 
            .ToList();
    }
    
    public async Task SetPricesForOrdersAsyncFromBitrix(List<Order> orders)
    {
        // Берем артикулы из ProductKey (до "=")
        var articles = orders
            .Where(o => !string.IsNullOrWhiteSpace(o.ProductKey))
            .Select(o => o.ProductKey!.Split('=')[0].Trim())
            .Where(a => !string.IsNullOrWhiteSpace(a))
            .Distinct()
            .ToList();
        
        var sql = @"
            SELECT 
                p.VALUE AS Article,
                price.PRICE AS Price
            FROM b_iblock_element e
            LEFT JOIN b_iblock_element_property p 
                ON e.ID = p.IBLOCK_ELEMENT_ID AND p.DESCRIPTION = 'Артикул'
            LEFT JOIN b_catalog_price price
                ON price.PRODUCT_ID = e.ID AND price.CATALOG_GROUP_ID = 3
            WHERE e.IBLOCK_ID = 93
              AND p.VALUE IN ({0})";

        // Подготавливаем параметры для IN
        var parameters = articles.Select((a, i) => new MySqlParameter($"@p{i}", a)).ToArray();
        var inClause = string.Join(",", parameters.Select(p => p.ParameterName));
        var formattedSql = string.Format(sql, inClause);

        // Выполняем запрос
        var results = await _context.Database
            .SqlQueryRaw<ArticlePriceResult>(formattedSql, parameters)
            .ToListAsync();

        // Словарь "Артикул -> Цена"
        var priceDictionary = results
            .Where(r => !string.IsNullOrEmpty(r.Article))
            .GroupBy(r => r.Article)
            .ToDictionary(
                g => g.Key,
                g => g.First().Price.HasValue 
                        ? (decimal?)Math.Round((decimal)g.First().Price.Value, 2) 
                        : null
            );

        // Проставляем цены в заказы
        foreach (var order in orders)
        {
            if (string.IsNullOrWhiteSpace(order.ProductKey))
                continue;

            var article = order.ProductKey.Split('=')[0].Trim();

            if (priceDictionary.TryGetValue(article, out var price))
            {
                order.PurchasePrice = price;
            }
        }
    }
    
    public async Task<Dictionary<int, List<StockInfo>>> SetStocksForOrdersAsyncFromBitrix(List<Order> orders)
    {
        // Берем артикулы из ProductKey (до "=")
        var articles = orders
            .Where(o => !string.IsNullOrWhiteSpace(o.ProductKey))
            .Select(o => o.ProductKey!.Split('=')[0].Trim())
            .Where(a => !string.IsNullOrWhiteSpace(a))
            .Distinct()
            .ToList();

        var sql = @"
            SELECT 
                CAST(e.ID AS SIGNED) AS ProductId,
                p.VALUE AS Article,
                e.PREVIEW_TEXT AS PreviewText,
                e.ACTIVE AS Active,
                e.TIMESTAMP_X AS TimestampX,
                c.QUANTITY AS Quantity,
                CAST(c.AVAILABLE AS CHAR) AS Available,
                supplier_enum.VALUE AS Supplier,
                price.PRICE AS Price,
                sp.STORE_ID AS StoreId,
                s.TITLE AS StoreTitle,
                sp.AMOUNT AS Amount
            FROM b_iblock_element e
            LEFT JOIN b_iblock_element_property p 
                ON e.ID = p.IBLOCK_ELEMENT_ID AND p.DESCRIPTION = 'Артикул'
            LEFT JOIN b_catalog_product c 
                ON c.ID = e.ID
            LEFT JOIN b_iblock_element_property prop_supplier 
                ON e.ID = prop_supplier.IBLOCK_ELEMENT_ID AND prop_supplier.IBLOCK_PROPERTY_ID = 2458
            LEFT JOIN b_iblock_property_enum supplier_enum 
                ON prop_supplier.VALUE = supplier_enum.ID AND supplier_enum.PROPERTY_ID = 2458
            LEFT JOIN b_catalog_price price
                ON price.PRODUCT_ID = e.ID AND price.CATALOG_GROUP_ID = 3
            LEFT JOIN b_catalog_store_product sp
                ON sp.PRODUCT_ID = e.ID
            LEFT JOIN b_catalog_store s
                ON sp.STORE_ID = s.ID
            WHERE e.IBLOCK_ID = 93
              AND p.VALUE IN ({0})
              AND sp.AMOUNT > 0
            GROUP BY e.ID, e.PREVIEW_TEXT, e.ACTIVE, e.TIMESTAMP_X, 
                     c.QUANTITY, c.AVAILABLE, supplier_enum.VALUE, price.PRICE, 
                     sp.STORE_ID, s.TITLE, sp.AMOUNT";

        // Подготавливаем параметры для IN
        var parameters = articles.Select((a, i) => new MySqlParameter($"@p{i}", a)).ToArray();
        var inClause = string.Join(",", parameters.Select(p => p.ParameterName));
        var formattedSql = string.Format(sql, inClause);

        // Выполняем запрос
        var results = await _context.Set<RemainingStockFlat>()
            .FromSqlRaw(formattedSql, parameters)
            .ToListAsync();

        // Группируем по артикулу и преобразуем в удобную структуру
        var stocksDictionary = results
            .Where(r => !string.IsNullOrEmpty(r.Article))
            .GroupBy(r => r.Article)
            .ToDictionary(
                g => g.Key,
                g => new
                {
                    TotalAmount = g.Sum(x => x.Amount), // Общий остаток по всем складам
                    Stores = g.Select(x => new StockInfo
                    {
                        StoreId = x.StoreId,
                        StoreTitle = x.StoreTitle,
                        Amount = x.Amount
                    }).ToList()
                }
            );
        
        Dictionary<int, List<StockInfo>> result = new Dictionary<int, List<StockInfo>>();
        // Проставляем остатки в заказы
        foreach (var order in orders)
        {
            if (string.IsNullOrWhiteSpace(order.ProductKey))
                continue;

            var article = order.ProductKey.Split('=')[0].Trim();

            if (stocksDictionary.TryGetValue(article, out var stockInfo))
            {
                if (stockInfo.Stores.Count() > 1)
                {
                    continue;
                }
                var targetSupplier = await _supplierRepository.GetAsync(new Supplier(){Name = stockInfo.Stores.FirstOrDefault()?.StoreTitle});
                if (targetSupplier != null)
                {
                    order.Supplier = targetSupplier;
                    order.SupplierId = targetSupplier.Id;
                }
                result.Add(order.Id, stockInfo.Stores);
            }
        }
        return result;
    }

    /// <summary>
    /// Обновляет поле ACTIVE в таблице b_iblock_element по ID элемента
    /// </summary>
    /// <param name="elementId">ID элемента</param>
    /// <param name="isActive">true - активировать ("Y"), false - деактивировать ("N")</param>
    /// <returns>Количество обновленных записей</returns>
    public async Task<int> UpdateElementActiveStatus(int elementId, bool isActive)
    {
        var activeValue = isActive ? "Y" : "N";
        
        var sql = @"
            UPDATE b_iblock_element 
            SET ACTIVE = {0}, TIMESTAMP_X = NOW()
            WHERE ID = {1} AND IBLOCK_ID = 93
        ";

        return await _context.Database.ExecuteSqlRawAsync(sql, activeValue, elementId);
    }

    /// <summary>
    /// Обновляет поле ACTIVE для нескольких элементов
    /// </summary>
    /// <param name="elementIds">Список ID элементов</param>
    /// <param name="isActive">true - активировать ("Y"), false - деактивировать ("N")</param>
    /// <returns>Количество обновленных записей</returns>
    public async Task<int> UpdateElementsActiveStatus(List<int> elementIds, bool isActive)
    {
        if (elementIds == null || !elementIds.Any())
        {
            return 0;
        }

        var activeValue = isActive ? "Y" : "N";
        var idsString = string.Join(",", elementIds);
        
        var sql = $@"
            UPDATE b_iblock_element 
            SET ACTIVE = {{0}}, TIMESTAMP_X = NOW()
            WHERE ID IN ({idsString}) AND IBLOCK_ID = 93
        ";

        return await _context.Database.ExecuteSqlRawAsync(sql, activeValue);
    }

    /// <summary>
    /// Получает текущее значение ACTIVE для элемента как bool
    /// </summary>
    /// <param name="elementId">ID элемента</param>
    /// <returns>true если активен, false если не активен, null если элемент не найден</returns>
    public async Task<bool?> GetElementActiveStatus(int elementId)
    {
        await using var command = _context.Database.GetDbConnection().CreateCommand();
    
        if (command.Connection?.State != System.Data.ConnectionState.Open)
        {
            await _context.Database.OpenConnectionAsync();
        }

        command.CommandText = @"
            SELECT ACTIVE 
            FROM b_iblock_element 
            WHERE ID = @elementId AND IBLOCK_ID = 93
        ";

        // Добавляем параметр для безопасности от SQL-инъекций
        var parameter = command.CreateParameter();
        parameter.ParameterName = "@elementId";
        parameter.Value = elementId;
        command.Parameters.Add(parameter);

        try
        {
            var result = await command.ExecuteScalarAsync();
            if (result == null || result == DBNull.Value)
            {
                return null;
            }

            var activeValue = result.ToString();
            return activeValue == "Y";
        }
        finally
        {
            await _context.Database.CloseConnectionAsync();
        }
    }

    /// <summary>
    /// Переключает значение ACTIVE (true -> false, false -> true)
    /// </summary>
    /// <param name="elementId">ID элемента</param>
    /// <returns>Новое значение ACTIVE как bool или null если элемент не найден</returns>
    public async Task<bool?> ToggleElementActiveStatus(int elementId)
    {
        var currentStatus = await GetElementActiveStatus(elementId);
        if (currentStatus == null)
        {
            return null;
        }

        var newStatus = !currentStatus.Value;
        await UpdateElementActiveStatus(elementId, newStatus);
        
        return newStatus;
    }
    
    private static Expression<Func<RemainingStockFlat, object>> GetSortPropertyFlat(string sortColumn)
    {
        return sortColumn.ToLower() switch
        {
            "productid"   => x => x.ProductId,
            "article"     => x => x.Article ?? "",
            "previewtext" => x => x.PreviewText ?? "",
            "active"      => x => x.Active,
            "available"   => x => x.Available,
            "supplier"    => x => x.Supplier ?? "",
            "timestampx"  => x => x.TimestampX,
            "quantity"    => x => x.Quantity,
            "price"       => x => x.Price ?? 0,

            // поля по складам
            "storeid"     => x => x.StoreId,
            "storetitle"  => x => x.StoreTitle ?? "",
            "amount"      => x => x.Amount,

            _             => x => x.TimestampX // дефолтная сортировка
        };
    }

}