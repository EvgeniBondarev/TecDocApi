using Swashbuckle.AspNetCore.Filters;
using TecDocApi.Application.Models;

namespace TecDocApi.API.Examples;

/// <summary>
/// Пример запроса поиска артикулов через Elasticsearch
/// </summary>
public class ArticleSearchRequestExample : IExamplesProvider<ArticleSearchRequest>
{
    public ArticleSearchRequest GetExamples()
    {
        return new ArticleSearchRequest
        {
            Query = "12345",
            Page = 1,
            PageSize = 20,
            SortBy = "relevance",
            SortDescending = false
        };
    }
}

