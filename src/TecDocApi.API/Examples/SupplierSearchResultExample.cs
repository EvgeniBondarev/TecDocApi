using Swashbuckle.AspNetCore.Filters;
using TecDocApi.Application.Models;

namespace TecDocApi.API.Examples;

/// <summary>
/// Пример ответа для поиска поставщиков через Elasticsearch
/// </summary>
public class SupplierSearchResultExample : IExamplesProvider<SupplierSearchResult>
{
    public SupplierSearchResult GetExamples()
    {
        return new SupplierSearchResult
        {
            Items = new List<SupplierDocument>
            {
                new SupplierDocument
                {
                    Id = "7",
                    SupplierId = 7,
                    Description = "BOSCH",
                    Matchcode = "BOS",
                    DataVersion = 2024,
                    NbrOfArticles = 15000,
                    HasNewVersionArticles = true,
                    IndexedAt = DateTime.UtcNow,
                    LastModified = DateTime.UtcNow
                }
            },
            Total = 1,
            Page = 1,
            PageSize = 20,
            TotalPages = 1,
            Took = 8
        };
    }
}

