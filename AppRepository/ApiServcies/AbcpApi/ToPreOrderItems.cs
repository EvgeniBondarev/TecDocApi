using Servcies.ApiServcies.TradesoftApi.Models.Response;

public static class AbcpArticleExtensions
{
    public static List<PreOrderItem> ToPreOrderItems(this IEnumerable<ArticleResponse> articles)
    {
        return AbcpArticleConverter.ConvertToPreOrderItems(articles);
    }

    public static PreOrderItem ToPreOrderItem(this ArticleResponse article)
    {
        return AbcpArticleConverter.ConvertSingleItem(article);
    }
}