using Swashbuckle.AspNetCore.Filters;
using TecDocApi.Application.Models;

namespace TecDocApi.API.Examples;

/// <summary>
/// Пример запроса поиска поставщиков через Elasticsearch
/// </summary>
public class SupplierSearchRequestExample : IExamplesProvider<SupplierSearchRequest>
{
    public SupplierSearchRequest GetExamples()
    {
        return new SupplierSearchRequest
        {
            Query = "BOSCH",
            Page = 1,
            PageSize = 20,
            SortBy = "relevance",
            SortDescending = false
        };
    }
}

