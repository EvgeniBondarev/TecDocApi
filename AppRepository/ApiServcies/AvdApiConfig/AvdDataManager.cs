using System.Xml.Linq;
using Microsoft.Extensions.Caching.Memory;
using Servcies.ApiServcies.AvdApiConfig;

public class AvdDataManager
{
    private readonly AvdApiConfig _config;
    private readonly AvdApiClient _client;
    private readonly IMemoryCache _cache;

    public AvdDataManager(AvdApiConfig config, IMemoryCache cache)
    {
        _config = config;
        _client = new AvdApiClient();
        _cache = cache;
    }

    public async Task<List<AvdPriceItem>> GetOriginalPriceAsync(string number, string catalog = "", string supplier = "")
    {
        var cacheKey = $"Avd_GetOriginalPrice_{number}_{catalog}_{supplier}";
        if (_cache.TryGetValue(cacheKey, out List<AvdPriceItem> cached))
            return cached;

        string soapRequest = $@"<?xml version=""1.0"" encoding=""utf-8""?>
                                <soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/""
                                               xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""
                                               xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
                                  <soap:Body>
                                    <GetOriginalPrice xmlns=""http://tempuri.org/"">
                                      <login>{_config.Login}</login>
                                      <password>{_config.Password}</password>
                                      <number>{number}</number>
                                      <catalog>{catalog}</catalog>
                                      <supplier></supplier>
                                    </GetOriginalPrice>
                                  </soap:Body>
                                </soap:Envelope>";

        var xDoc = await _client.MakeSoapRequestAsync(
            AvdApiUrl.SOAP_ACTION_GET_ORIGINAL_PRICE,
            soapRequest
        );

        XNamespace a = "http://schemas.datacontract.org/2004/07/Avd.Service.Models";
        var items = xDoc.Descendants(a + "PriceItem3")
            .Select(x => new AvdPriceItem
            {
                CatalogName = x.Element(a + "CatalogName")?.Value,
                DatePrice = DateTime.TryParse(x.Element(a + "DatePrice")?.Value, out var date) ? date : null,
                DealerStore = x.Element(a + "DealerStore")?.Value,
                Hash = x.Element(a + "Hash")?.Value,
                IsOriginal = bool.TryParse(x.Element(a + "IsOriginal")?.Value, out var orig) && orig,
                ItemName = x.Element(a + "ItemName")?.Value,
                ItemNumber = x.Element(a + "ItemNumber")?.Value,
                Multiply = int.TryParse(x.Element(a + "Multiply")?.Value, out var mult) ? mult : 0,
                Price = decimal.TryParse(x.Element(a + "Price")?.Value, out var price) ? price : 0,
                PriceAverage = x.Element(a + "PriceAverage")?.Value,
                PriceStatistic = int.TryParse(x.Element(a + "PriceStatistic")?.Value, out var stat) ? stat : 0,
                Quantity = int.TryParse(x.Element(a + "Quantity")?.Value, out var qty) ? qty : 0,
                SupplierCastrol = x.Element(a + "SupplierCastrol")?.Value,
                SupplierInfo = x.Element(a + "SupplierInfo")?.Value,
                SupplierName = x.Element(a + "SupplierName")?.Value,
                SupplierOfficial = x.Element(a + "SupplierOfficial")?.Value,
                SupplierPeriod = x.Element(a + "SupplierPeriod")?.Value,
                SupplierPrepay = decimal.TryParse(x.Element(a + "SupplierPrepay")?.Value, out var prepay) ? prepay : 0,
                SupplierRecommended = int.TryParse(x.Element(a + "SupplierRecommended")?.Value, out var rec) ? rec : 0,
                SupplierRegion = x.Element(a + "SupplierRegion")?.Value,
                SupplierRestrictions = x.Element(a + "SupplierRestrictions")?.Value,
                SupplierReturn = int.TryParse(x.Element(a + "SupplierReturn")?.Value, out var ret) ? ret : 0,
                SupplierReturnDescription = x.Element(a + "SupplierReturn_description")?.Value,
                SupplierSendGraf1 = x.Element(a + "SupplierSendGraf1")?.Value,
                SupplierSendGraf2 = x.Element(a + "SupplierSendGraf2")?.Value,
                SupplierSendGraf3 = x.Element(a + "SupplierSendGraf3")?.Value,
                SupplierSendGraf4 = x.Element(a + "SupplierSendGraf4")?.Value,
                SupplierSendGraf5 = x.Element(a + "SupplierSendGraf5")?.Value,
                SupplierSendGraf6 = x.Element(a + "SupplierSendGraf6")?.Value,
                SupplierSendGraf7 = x.Element(a + "SupplierSendGraf7")?.Value,
                SupplierSpecification = x.Element(a + "SupplierSpecification")?.Value,
                SupplierStock = int.TryParse(x.Element(a + "SupplierStock")?.Value, out var stock) ? stock : 0
            })
            .ToList();

        _cache.Set(cacheKey, items, TimeSpan.FromMinutes(30));
        return items;
    }
}
