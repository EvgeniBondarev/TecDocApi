using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json.Linq;
using OzonDomains;
using OzonDomains.Models;
using OzonRepositories.Data;
using Servcies.ParserServcies;
using Servcies.ParserServcies.HelpDictEnum;
using Services.CacheServcies.Cache;

namespace Servcies.DataServcies;

public class StockDataService 
{
    private readonly CsvUrlParser _urlParser;
    private readonly StocksCaster _stocksCaster;
    private readonly AppCache _appCache;
    private readonly SupplierDataServcies _supplierDataServcies;

    public StockDataService(CsvUrlParser urlParser, 
                            StocksCaster stocksCaster, 
                            AppCache appCache,
                            SupplierDataServcies supplierDataServcies)
    {
        _urlParser = urlParser;
        _stocksCaster = stocksCaster;
        _appCache = appCache;
        _supplierDataServcies = supplierDataServcies;
    }

    public async Task<List<StocksData>> GetStocksDataByOrder(Order order)
    {
        List<StocksData> stocksDataResult = new();
        if (order.Article != null)
        {
            try
            {
                List<Supplier> suppliers = await _supplierDataServcies.GetSuppliers();
                var cache = _appCache.GetCache();
                foreach (Supplier supplier in suppliers)
                {
                    if (supplier.CsvUrl != null)
                    {
                        string cacheKey = supplier.CsvUrl;
                        if (!cache.TryGetValue(cacheKey, out JArray jsonStock))
                        {
                            jsonStock = await _urlParser.ReadFileFromUrl(supplier.CsvUrl);

                            if (jsonStock != null)
                            {
                                var cacheEntryOptions = new MemoryCacheEntryOptions
                                {
                                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
                                };

                                cache.Set(cacheKey, jsonStock, cacheEntryOptions);
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Stock found in cache [{cacheKey}]");
                        }
                        var searchResult = _stocksCaster.Search(jsonStock, order.Article);
                        stocksDataResult.AddRange(await _stocksCaster.ParseToStocksData(searchResult,supplier.Name));
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        return stocksDataResult;
    }
}
