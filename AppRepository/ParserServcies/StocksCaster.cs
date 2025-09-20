using Newtonsoft.Json.Linq;
using OzonDomains;

namespace Servcies.ParserServcies.HelpDictEnum;

public class StocksCaster
{
    public StocksCaster()
    {
        
    }
    public async Task<List<StocksData>> ParseToStocksData(List<JObject> jsonList, string? stockName)
    {
        var result = jsonList.Select(json => new StocksData
        {
            Manufacturer = json["Производитель"]?.ToString(),
            Article = json["Артикул"]?.ToString(),
            Stock = json["Остаток"] != null && int.TryParse(json["Остаток"]?.ToString(), out int stock) ? stock : (int?)null,
            Price = json["Цена"] != null && decimal.TryParse(json["Цена"]?.ToString(), out decimal price) ? price : (decimal?)null,
            Name = json["Наименование"]?.ToString(),
            StockName = stockName
        }).ToList();

        return await Task.FromResult(result);
    }
    
    public List<JObject> Search(JArray data, string article)
    {
        return data
            .OfType<JObject>()
            .Where(item =>
                item.TryGetValue("Артикул", out var art) && art.ToString() == article
            )
            .ToList();
    }
    
}