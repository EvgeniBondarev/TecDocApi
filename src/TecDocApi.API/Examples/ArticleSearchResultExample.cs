using Swashbuckle.AspNetCore.Filters;
using TecDocApi.Application.Models;

namespace TecDocApi.API.Examples;

/// <summary>
/// Пример ответа для поиска артикулов через Elasticsearch
/// </summary>
public class ArticleSearchResultExample : IExamplesProvider<ArticleSearchResult>
{
    public ArticleSearchResult GetExamples()
    {
        return new ArticleSearchResult
        {
            Items = new List<ArticleDocument>
            {
                new ArticleDocument
                {
                    Id = "7_12345",
                    SupplierId = 7,
                    DataSupplierArticleNumber = "12345",
                    FoundString = "12345",
                    NormalizedDescription = "Фильтр масляный",
                    Description = "Для легковых автомобилей",
                    ArticleStateDisplayValue = "Normal",
                    QuantityPerPackingUnit = 1,
                    SupplierDescription = "BOSCH",
                    SupplierMatchcode = "BOS",
                    IndexedAt = DateTime.UtcNow,
                    LastModified = DateTime.UtcNow
                }
            },
            Total = 1,
            Page = 1,
            PageSize = 20,
            TotalPages = 1,
            Took = 15
        };
    }
}

