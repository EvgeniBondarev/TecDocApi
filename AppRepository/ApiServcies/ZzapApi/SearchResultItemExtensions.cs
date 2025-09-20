using System.Collections.Generic;
using Servcies.ApiServcies.TradesoftApi.Models.Response;

public static class SearchResultItemExtensions
{
    public static List<PreOrderItem> ToPreOrderItems(this IEnumerable<SearchResultItem> sourceItems)
    {
        return SearchResultItemConverter.ConvertToPreOrderItems(sourceItems);
    }

    public static PreOrderItem ToPreOrderItem(this SearchResultItem sourceItem)
    {
        return SearchResultItemConverter.ConvertSingleItem(sourceItem);
    }

    public static List<PreOrderItem> ToPreOrderItems(this IEnumerable<SearchResultItem> sourceItems, string siteUrl, string title)
    {
        var items = SearchResultItemConverter.ConvertToPreOrderItems(sourceItems);
        items.ForEach(item => 
        {
            item.SiteUrl = siteUrl;
            item.Title = title;
        });
        return items;
    }

    public static PreOrderItem ToPreOrderItem(this SearchResultItem sourceItem, string siteUrl, string title)
    {
        var item = SearchResultItemConverter.ConvertSingleItem(sourceItem);
        if (item != null)
        {
            item.SiteUrl = siteUrl;
            item.Title = title;
        }
        return item;
    }
}