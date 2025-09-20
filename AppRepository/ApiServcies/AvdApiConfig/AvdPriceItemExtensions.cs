using Servcies.ApiServcies.TradesoftApi.Models.Response;

namespace Servcies.ApiServcies.AvdApiConfig;

public static class AvdPriceItemExtensions
{
    public static List<PreOrderItem> ToPreOrderItems(this IEnumerable<AvdPriceItem> avdPriceItems)
    {
        return AvdPriceItemConverter.ConvertToPreOrderItems(avdPriceItems);
    }

    public static PreOrderItem ToPreOrderItem(this AvdPriceItem avdPriceItem)
    {
        return AvdPriceItemConverter.ConvertSingleItem(avdPriceItem);
    }
}