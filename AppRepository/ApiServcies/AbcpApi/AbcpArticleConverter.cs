using System.Globalization;
using Servcies.ApiServcies.TradesoftApi.Models.Response;

public static class AbcpArticleConverter
{
    public static List<PreOrderItem> ConvertToPreOrderItems(IEnumerable<ArticleResponse> sourceItems)
    {
        if (sourceItems == null)
            return new List<PreOrderItem>();

        return sourceItems
            .Where(item => item != null)
            .Select(ConvertSingleItem)
            .ToList();
    }

    public static PreOrderItem ConvertSingleItem(ArticleResponse source)
    {
        if (source == null)
            return null;

        return new PreOrderItem
        {
            Caption = source.Description ?? string.Empty,
            Rest = source.Availability?.ToString() ?? "0",  
            DeliveryDays =  ((int)Math.Ceiling((source.DeliveryPeriod ?? 0) / 24.0)).ToString(),
            DeliveryDaysMin = (int)Math.Ceiling((source.DeliveryPeriod ?? 0) / 24.0),
            DeliveryDaysMax = (int)Math.Ceiling((source.DeliveryPeriodMax ?? source.DeliveryPeriod ?? 0) / 24.0),
            CurrencyCode = "RUB",
            Producer = source.Brand ?? string.Empty,
            Code = source.Number ?? string.Empty,
            Price = source.Price.ToString(CultureInfo.InvariantCulture),
            Direction = source.SupplierDescription ?? string.Empty,
            ItemHash = GenerateItemHash(source),
            ItemId = GenerateItemId(source),
            SiteUrl = "https://www.abcp.ru/",
            Title = source.DistributorCode ?? string.Empty,
            Description = $"{source.Description}\n" +
                          $"Последнее обновление: {source.LastUpdateTime}\n"
        };
    }

    private static string GenerateItemHash(ArticleResponse item)
    {
        return $"{item.DistributorId}_{item.Number}_{item.Brand}".GetHashCode().ToString("X");
    }

    private static string GenerateItemId(ArticleResponse item)
    {
        return $"{item.DistributorId}_{item.Number}_{item.SupplierCode}";
    }
}