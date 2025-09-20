using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Servcies.PriceСlculationServcies
{
    public class CurrencyRateFetcher
    {
        private static readonly HttpClient client = new HttpClient();
        private static readonly ConcurrentDictionary<string, decimal> currencyRatesCache = new ConcurrentDictionary<string, decimal>();

        public async Task<decimal> GetUSDRateAsync() => await GetCurrencyRateAsync("USD");
        public async Task<decimal> GetEURRateAsync() => await GetCurrencyRateAsync("EUR");
        public async Task<decimal> GetBYNRateAsync() => await GetCurrencyRateAsync("BYN");

        private async Task<decimal> GetCurrencyRateAsync(string currencyCode)
        {
            if (currencyRatesCache.TryGetValue(currencyCode, out var cachedRate))
            {
                return cachedRate;
            }

            try
            {
                string url = $"https://www.cbr-xml-daily.ru/daily_json.js";
                var response = await client.GetStringAsync(url);
                var json = JObject.Parse(response);
                
                var rate = json["Valute"][currencyCode]["Value"].Value<decimal>();
                
                currencyRatesCache[currencyCode] = rate;

                return rate;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                throw;
            }
        }

        public void ClearCache()
        {
            currencyRatesCache.Clear();
        }
    }
}