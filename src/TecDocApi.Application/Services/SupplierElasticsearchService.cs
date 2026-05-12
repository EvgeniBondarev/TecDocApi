using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nest;
using TecDocApi.Application.Models;

namespace TecDocApi.Application.Services;

/// <summary>
/// Реализация сервиса для работы с Elasticsearch - поиск и индексация поставщиков
/// </summary>
public class SupplierElasticsearchService : ISupplierElasticsearchService
{
    private readonly IElasticClient _client;
    private readonly string _indexName;
    private readonly ILogger<SupplierElasticsearchService> _logger;

    public SupplierElasticsearchService(IConfiguration configuration, ILogger<SupplierElasticsearchService> logger)
    {
        _logger = logger;
        _indexName = configuration["Elasticsearch:SupplierIndexName"] ?? "suppliers";
        
        var elasticsearchUrl = configuration["Elasticsearch:Url"] ?? "http://localhost:9200";
        var settings = new ConnectionSettings(new Uri(elasticsearchUrl))
            .DefaultIndex(_indexName)
            .RequestTimeout(TimeSpan.FromMinutes(2))
            .EnableDebugMode()
            .PrettyJson();
        
        var username = configuration["Elasticsearch:Username"];
        var password = configuration["Elasticsearch:Password"];
        
        if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
        {
            settings.BasicAuthentication(username, password);
        }
        
        _client = new ElasticClient(settings);
    }

    public async Task<bool> CreateIndexAsync()
    {
        try
        {
            var exists = await _client.Indices.ExistsAsync(_indexName);
            
            if (exists.Exists)
            {
                _logger.LogInformation("Индекс {IndexName} уже существует", _indexName);
                return true;
            }

            var createIndexResponse = await _client.Indices.CreateAsync(_indexName, c => c
                .Map<SupplierDocument>(m => m
                    .Properties(p => p
                        .Keyword(k => k.Name(d => d.Id))
                        .Number(n => n.Name(d => d.SupplierId).Type(NumberType.Short))
                        .Text(t => t.Name(d => d.Description)
                            .Analyzer("russian")
                            .Fields(f => f
                                .Keyword(k => k.Name("keyword"))
                                .Text(tt => tt.Name("english").Analyzer("english"))
                            ))
                        .Text(t => t.Name(d => d.Matchcode)
                            .Analyzer("standard")
                            .Fields(f => f
                                .Keyword(k => k.Name("keyword"))
                                .Text(tt => tt.Name("ngram").Analyzer("ngram_analyzer"))
                            ))
                        .Number(n => n.Name(d => d.DataVersion).Type(NumberType.Short))
                        .Number(n => n.Name(d => d.NbrOfArticles).Type(NumberType.Integer))
                        .Boolean(b => b.Name(d => d.HasNewVersionArticles))
                        .Date(d => d.Name(doc => doc.IndexedAt))
                        .Date(d => d.Name(doc => doc.LastModified))
                    )
                )
                .Settings(s => s
                    .Analysis(a => a
                        .Analyzers(an => an
                            .Custom("russian", ca => ca
                                .Tokenizer("standard")
                                .Filters("lowercase", "russian_stop", "russian_stemmer")
                            )
                            .Custom("ngram_analyzer", ca => ca
                                .Tokenizer("keyword")
                                .Filters("lowercase", "ngram_filter")
                            )
                        )
                        .TokenFilters(tf => tf
                            .Stop("russian_stop", st => st
                                .StopWords("_russian_")
                            )
                            .Stemmer("russian_stemmer", st => st
                                .Language("russian")
                            )
                            .NGram("ngram_filter", ng => ng
                                .MinGram(2)
                                .MaxGram(10)
                            )
                        )
                    )
                    .NumberOfShards(1)
                    .NumberOfReplicas(1)
                )
            );

            if (createIndexResponse.IsValid)
            {
                _logger.LogInformation("Индекс {IndexName} успешно создан", _indexName);
                return true;
            }
            
            _logger.LogError("Ошибка создания индекса: {DebugInfo}", createIndexResponse.DebugInformation);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при создании индекса {IndexName}", _indexName);
            return false;
        }
    }

    public async Task<bool> IndexSupplierAsync(SupplierDocument supplier)
    {
        try
        {
            supplier.Id = supplier.SupplierId.ToString();
            supplier.IndexedAt = DateTime.UtcNow;
            
            var response = await _client.IndexDocumentAsync(supplier);
            
            if (response.IsValid)
            {
                _logger.LogDebug("Поставщик {SupplierId} проиндексирован", supplier.SupplierId);
                return true;
            }
            
            _logger.LogError("Ошибка индексации поставщика {SupplierId}: {DebugInfo}", supplier.SupplierId, response.DebugInformation);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при индексации поставщика {SupplierId}", supplier.SupplierId);
            return false;
        }
    }

    public async Task<bool> BulkIndexSuppliersAsync(IEnumerable<SupplierDocument> suppliers)
    {
        try
        {
            var bulkDescriptor = new BulkDescriptor();
            
            foreach (var supplier in suppliers)
            {
                supplier.Id = supplier.SupplierId.ToString();
                supplier.IndexedAt = DateTime.UtcNow;
                
                bulkDescriptor.Index<SupplierDocument>(i => i
                    .Document(supplier)
                    .Id(supplier.Id)
                );
            }
            
            var response = await _client.BulkAsync(bulkDescriptor);
            
            if (response.IsValid)
            {
                _logger.LogInformation("Успешно проиндексировано {Count} поставщиков", response.Items.Count);
                return true;
            }
            
            _logger.LogError("Ошибка массовой индексации: {DebugInfo}", response.DebugInformation);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при массовой индексации поставщиков");
            return false;
        }
    }

    public async Task<SupplierSearchResult> SearchSuppliersAsync(SupplierSearchRequest request)
    {
        var result = new SupplierSearchResult
        {
            Page = request.Page ?? 1,
            PageSize = request.PageSize ?? 20
        };

        try
        {
            var searchDescriptor = new SearchDescriptor<SupplierDocument>()
                .Query(q => BuildSearchQuery(request))
                .From(request.Skip)
                .Size(request.PageSize ?? 20)
                .Sort(s => BuildSort(request));

            var searchResponse = await _client.SearchAsync<SupplierDocument>(searchDescriptor);
            
            if (!searchResponse.IsValid)
            {
                _logger.LogError("Ошибка поиска: {DebugInfo}", searchResponse.DebugInformation);
                return result;
            }

            result.Items = searchResponse.Documents.ToList();
            result.Total = searchResponse.Total;
            result.TotalPages = (int)Math.Ceiling((double)result.Total / result.PageSize);
            result.Took = searchResponse.Took;

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при выполнении поиска");
            return result;
        }
    }

    public async Task<bool> DeleteIndexAsync()
    {
        try
        {
            var response = await _client.Indices.DeleteAsync(_indexName);
            
            if (response.IsValid)
            {
                _logger.LogInformation("Индекс {IndexName} удален", _indexName);
                return true;
            }
            
            _logger.LogError("Ошибка удаления индекса: {DebugInfo}", response.DebugInformation);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при удалении индекса {IndexName}", _indexName);
            return false;
        }
    }

    public async Task<long> GetTotalCountAsync()
    {
        try
        {
            var countResponse = await _client.CountAsync<SupplierDocument>();
            return countResponse.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении количества документов");
            return 0;
        }
    }

    public async Task<bool> IndexExistsAsync()
    {
        try
        {
            var response = await _client.Indices.ExistsAsync(_indexName);
            return response.Exists;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при проверке существования индекса");
            return false;
        }
    }

    #region Private Methods

    private QueryContainer BuildSearchQuery(SupplierSearchRequest request)
    {
        var queries = new List<QueryContainer>();

        if (!string.IsNullOrWhiteSpace(request.Query))
        {
            var normalizedQuery = request.Query.Trim().ToUpperInvariant();

            // Поиск по Description и Matchcode
            queries.Add(new BoolQuery
            {
                Should = new QueryContainer[]
                {
                    // Точное совпадение по Matchcode (высокий приоритет)
                    new TermQuery
                    {
                        Field = Infer.Field<SupplierDocument>(f => f.Matchcode),
                        Value = normalizedQuery,
                        Boost = 5.0
                    },
                    // Ngram поиск по Matchcode для частичного совпадения
                    new MatchQuery
                    {
                        Field = Infer.Field<SupplierDocument>(f => f.Matchcode.Suffix("ngram")),
                        Query = normalizedQuery,
                        Boost = 4.0,
                        Operator = Operator.And
                    },
                    // Поиск по Description
                    new MatchQuery
                    {
                        Field = Infer.Field<SupplierDocument>(f => f.Description),
                        Query = request.Query,
                        Boost = 3.0,
                        Fuzziness = Fuzziness.Auto,
                        Operator = Operator.Or
                    },
                    // Точное совпадение по Description (keyword)
                    new MatchQuery
                    {
                        Field = Infer.Field<SupplierDocument>(f => f.Description.Suffix("keyword")),
                        Query = request.Query,
                        Boost = 2.0,
                        Operator = Operator.And
                    }
                },
                MinimumShouldMatch = 1
            });
        }

        return queries.Any() 
            ? new BoolQuery { Must = queries } 
            : new MatchAllQuery();
    }

    private SortDescriptor<SupplierDocument> BuildSort(SupplierSearchRequest request)
    {
        var sortDescriptor = new SortDescriptor<SupplierDocument>();

        switch (request.SortBy?.ToLower())
        {
            case "relevance":
            default:
                if (!string.IsNullOrWhiteSpace(request.Query))
                {
                    sortDescriptor.Field("_score", request.SortDescending ? SortOrder.Descending : SortOrder.Ascending);
                }
                else
                {
                    sortDescriptor.Field(f => f.SupplierId, request.SortDescending ? SortOrder.Descending : SortOrder.Ascending);
                }
                break;
            case "description":
                sortDescriptor.Field(f => f.Description, request.SortDescending ? SortOrder.Descending : SortOrder.Ascending);
                break;
            case "matchcode":
                sortDescriptor.Field(f => f.Matchcode, request.SortDescending ? SortOrder.Descending : SortOrder.Ascending);
                break;
            case "nbrofarticles":
                sortDescriptor.Field(f => f.NbrOfArticles, request.SortDescending ? SortOrder.Descending : SortOrder.Ascending);
                break;
        }

        return sortDescriptor;
    }

    #endregion
}

