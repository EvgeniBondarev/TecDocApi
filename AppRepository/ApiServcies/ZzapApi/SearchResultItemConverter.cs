using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Servcies.ApiServcies.TradesoftApi.Models.Response;

public static class SearchResultItemConverter
{
    public static List<PreOrderItem> ConvertToPreOrderItems(IEnumerable<SearchResultItem> sourceItems)
    {
        if (sourceItems == null)
            return new List<PreOrderItem>();

        return sourceItems
            .Where(item => item != null)
            .Select(ConvertSingleItem)
            .ToList();
    }

    public static PreOrderItem ConvertSingleItem(SearchResultItem source)
    {
        if (source == null)
            return null;
        int minDays = 0;
        int maxDays = 0;
        bool hasNumber = false;
        string? priceDescription = null;

        if (!string.IsNullOrWhiteSpace(source.DeliveryTime))
        {
            var text = source.DeliveryTime.Trim().ToLower();

            // Список стоп-слов, которые не являются сроками в днях
            string[] stopWords = { "сегодня", "налич", "в наличии", "час", "завтра" };
            if (!stopWords.Any(w => text.Contains(w)))
            {
                // Находим все числа
                var matches = Regex.Matches(text, @"\d+");
                if (matches.Count >= 1)
                {
                    hasNumber = true;
                    int.TryParse(matches[0].Value, out minDays);

                    if (matches.Count >= 2)
                        int.TryParse(matches[1].Value, out maxDays);

                    if (maxDays == 0)
                        maxDays = minDays;
                }
            }
        }

        if (minDays == 0 && maxDays == 0)
        {
            maxDays = minDays = source.DeliveryDays;
        }

        if (source.IsUsedV2)
        {
            priceDescription = "Б/у или уценка";
        }
        
        return new PreOrderItem
        {
            Caption = source.Name ?? string.Empty,
            Rest = source.Quantity.ToString(),
            DeliveryDays = minDays.ToString(),
            DeliveryDaysMin = source.DeliveryDays,
            DeliveryDaysMax = maxDays,
            CurrencyCode = "RUB", 
            Producer = source.Brand ?? string.Empty,
            Code = source.PartNumber ?? string.Empty,
            Price = source.Price.ToString(CultureInfo.InvariantCulture),
            Direction = source.Warehouse,
            ItemHash = source.CodeCat.ToString() ?? string.Empty,
            ItemId = source.CodeDocB.ToString() ?? string.Empty,
            SiteUrl = "https://www.zzap.ru/", 
            Title = "ZZap.ru",
            PriceDescription = priceDescription,
            Description = $"{source.Name}" +
                          $"\n{source.DeliveryTime}" +
                          $"\n{source.DescrPrice}" +
                          $"\n{source.DescrRatingCount}" +
                          $"\n{source.DescrAddress}" +
                          $"\n{source.Apply}" +
                          $"\n{source.Shipment}"
        };
    }

    private static string GenerateItemHash(SearchResultItem item)
    {
        // Генерация уникального хеша на основе ключевых полей
        return $"{item.CodeDocB}_{item.CodeCat}_{item.PartNumber}".GetHashCode().ToString("X");
    }

    private static string GenerateItemId(SearchResultItem item)
    {
        // Генерация ID на основе ключевых полей
        return $"{item.CodeMan}_{item.PartNumber}_{item.CodeDocB}";
    }
    
}