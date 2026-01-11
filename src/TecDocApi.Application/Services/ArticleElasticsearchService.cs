using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nest;
using TecDocApi.Application.Models;

namespace TecDocApi.Application.Services;

/// <summary>
/// Реализация сервиса для работы с Elasticsearch - поиск и индексация артикулов
/// </summary>
public class ArticleElasticsearchService : IArticleElasticsearchService
{
    private readonly IElasticClient _client;
    private readonly string _indexName;
    private readonly ILogger<ArticleElasticsearchService> _logger;

    public ArticleElasticsearchService(IConfiguration configuration, ILogger<ArticleElasticsearchService> logger)
    {
        _logger = logger;
        _indexName = configuration["Elasticsearch:IndexName"] ?? "articles";
        
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
                .Map<ArticleDocument>(m => m
                    .Properties(p => p
                        .Keyword(k => k.Name(d => d.Id))
                        .Number(n => n.Name(d => d.SupplierId).Type(NumberType.Short))
                        .Keyword(k => k.Name(d => d.DataSupplierArticleNumber))
                        .Text(t => t.Name(d => d.FoundString)
                            .Analyzer("standard")
                            .Fields(f => f
                                .Keyword(k => k.Name("keyword"))
                                .Text(tt => tt.Name("ngram").Analyzer("ngram_analyzer"))
                            ))
                        .Text(t => t.Name(d => d.NormalizedDescription)
                            .Analyzer("russian")
                            .Fields(f => f
                                .Keyword(k => k.Name("keyword"))
                                .Text(tt => tt.Name("english").Analyzer("english"))
                            ))
                        .Text(t => t.Name(d => d.Description)
                            .Analyzer("russian"))
                        .Keyword(k => k.Name(d => d.ArticleStateDisplayValue))
                        .Number(n => n.Name(d => d.QuantityPerPackingUnit).Type(NumberType.Integer))
                        .Text(t => t.Name(d => d.SupplierDescription)
                            .Analyzer("russian")
                            .Fields(f => f
                                .Keyword(k => k.Name("keyword"))
                            ))
                        .Text(t => t.Name(d => d.SupplierMatchcode)
                            .Analyzer("standard")
                            .Fields(f => f
                                .Keyword(k => k.Name("keyword"))
                                .Text(tt => tt.Name("ngram").Analyzer("ngram_analyzer"))
                            ))
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
                    .NumberOfShards(2)
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

    public async Task<bool> IndexArticleAsync(ArticleDocument article)
    {
        try
        {
            article.Id = $"{article.SupplierId}_{article.DataSupplierArticleNumber}";
            article.IndexedAt = DateTime.UtcNow;
            
            var response = await _client.IndexDocumentAsync(article);
            
            if (response.IsValid)
            {
                _logger.LogDebug("Артикул {ArticleId} проиндексирован", article.Id);
                return true;
            }
            
            _logger.LogError("Ошибка индексации артикула {ArticleId}: {DebugInfo}", article.Id, response.DebugInformation);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при индексации артикула {ArticleId}", article.Id);
            return false;
        }
    }

    public async Task<bool> BulkIndexArticlesAsync(IEnumerable<ArticleDocument> articles)
    {
        try
        {
            var bulkDescriptor = new BulkDescriptor();
            
            foreach (var article in articles)
            {
                article.Id = $"{article.SupplierId}_{article.DataSupplierArticleNumber}";
                article.IndexedAt = DateTime.UtcNow;
                
                bulkDescriptor.Index<ArticleDocument>(i => i
                    .Document(article)
                    .Id(article.Id)
                );
            }
            
            var response = await _client.BulkAsync(bulkDescriptor);
            
            if (response.IsValid)
            {
                _logger.LogInformation("Успешно проиндексировано {Count} артикулов", response.Items.Count);
                return true;
            }
            
            _logger.LogError("Ошибка массовой индексации: {DebugInfo}", response.DebugInformation);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при массовой индексации артикулов");
            return false;
        }
    }

    public async Task<ArticleSearchResult> SearchArticlesAsync(ArticleSearchRequest request)
    {
        var result = new ArticleSearchResult
        {
            Page = request.Page ?? 1,
            PageSize = request.PageSize ?? 20
        };

        try
        {
            var searchDescriptor = new SearchDescriptor<ArticleDocument>()
                .Query(q => BuildSearchQuery(request))
                .From(request.Skip)
                .Size(request.PageSize ?? 20)
                .Sort(s => BuildSort(request));

            var searchResponse = await _client.SearchAsync<ArticleDocument>(searchDescriptor);
            
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
            var countResponse = await _client.CountAsync<ArticleDocument>();
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

    public async Task<ArticleSearchResult> SearchArticlesBySupplierAsync(ArticleSearchRequest request)
    {
        var result = new ArticleSearchResult
        {
            Page = request.Page ?? 1,
            PageSize = request.PageSize ?? 20
        };

        try
        {
            var searchDescriptor = new SearchDescriptor<ArticleDocument>()
                .Query(q => BuildSupplierSearchQuery(request))
                .From(request.Skip)
                .Size(request.PageSize ?? 20)
                .Sort(s => BuildSort(request));

            var searchResponse = await _client.SearchAsync<ArticleDocument>(searchDescriptor);
            
            if (!searchResponse.IsValid)
            {
                _logger.LogError("Ошибка поиска по поставщику: {DebugInfo}", searchResponse.DebugInformation);
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
            _logger.LogError(ex, "Ошибка при выполнении поиска по поставщику");
            return result;
        }
    }

    #region Private Methods

    private QueryContainer BuildSearchQuery(ArticleSearchRequest request)
    {
        var queries = new List<QueryContainer>();

        if (!string.IsNullOrWhiteSpace(request.Query))
        {
            var normalizedQuery = request.Query.Trim().ToUpperInvariant();

            // Поиск по FoundString (точное совпадение или ngram)
            queries.Add(new BoolQuery
            {
                Should = new QueryContainer[]
                {
                    // Точное совпадение (высокий приоритет)
                    new TermQuery
                    {
                        Field = Infer.Field<ArticleDocument>(f => f.FoundString),
                        Value = normalizedQuery,
                        Boost = 5.0
                    },
                    // Ngram поиск для частичного совпадения
                    new MatchQuery
                    {
                        Field = Infer.Field<ArticleDocument>(f => f.FoundString.Suffix("ngram")),
                        Query = normalizedQuery,
                        Boost = 3.0,
                        Operator = Operator.And
                    },
                    // Поиск по NormalizedDescription
                    new MatchQuery
                    {
                        Field = Infer.Field<ArticleDocument>(f => f.NormalizedDescription),
                        Query = request.Query,
                        Boost = 2.0,
                        Fuzziness = Fuzziness.Auto,
                        Operator = Operator.Or
                    },
                    // Поиск по Description
                    new MatchQuery
                    {
                        Field = Infer.Field<ArticleDocument>(f => f.Description),
                        Query = request.Query,
                        Boost = 1.0,
                        Fuzziness = Fuzziness.Auto,
                        Operator = Operator.Or
                    }
                },
                MinimumShouldMatch = 1
            });
        }

        // Фильтр по SupplierId
        if (request.SupplierId.HasValue)
        {
            queries.Add(new TermQuery
            {
                Field = Infer.Field<ArticleDocument>(f => f.SupplierId),
                Value = request.SupplierId.Value
            });
        }

        return queries.Any() 
            ? new BoolQuery { Must = queries } 
            : new MatchAllQuery();
    }

    private SortDescriptor<ArticleDocument> BuildSort(ArticleSearchRequest request)
    {
        var sortDescriptor = new SortDescriptor<ArticleDocument>();

        switch (request.SortBy?.ToLower())
        {
            case "relevance":
            default:
                if (!string.IsNullOrWhiteSpace(request.Query))
                {
                    sortDescriptor.Field("_score", request.SortDescending ? SortOrder.Descending : SortOrder.Descending);
                }
                else
                {
                    sortDescriptor.Field(f => f.SupplierId, request.SortDescending ? SortOrder.Descending : SortOrder.Ascending);
                }
                break;
            case "foundstring":
                sortDescriptor.Field(f => f.FoundString, request.SortDescending ? SortOrder.Descending : SortOrder.Ascending);
                break;
            case "description":
                sortDescriptor.Field(f => f.NormalizedDescription, request.SortDescending ? SortOrder.Descending : SortOrder.Ascending);
                break;
        }

        return sortDescriptor;
    }

    private QueryContainer BuildSupplierSearchQuery(ArticleSearchRequest request)
    {
        var queries = new List<QueryContainer>();

        if (!string.IsNullOrWhiteSpace(request.Query))
        {
            var normalizedQuery = request.Query.Trim().ToUpperInvariant();

            // Поиск по модели поставщика (SupplierDescription и SupplierMatchcode)
            queries.Add(new BoolQuery
            {
                Should = new QueryContainer[]
                {
                    // Точное совпадение по Matchcode (высокий приоритет)
                    new TermQuery
                    {
                        Field = Infer.Field<ArticleDocument>(f => f.SupplierMatchcode),
                        Value = normalizedQuery,
                        Boost = 5.0
                    },
                    // Ngram поиск по Matchcode для частичного совпадения
                    new MatchQuery
                    {
                        Field = Infer.Field<ArticleDocument>(f => f.SupplierMatchcode.Suffix("ngram")),
                        Query = normalizedQuery,
                        Boost = 4.0,
                        Operator = Operator.And
                    },
                    // Поиск по SupplierDescription
                    new MatchQuery
                    {
                        Field = Infer.Field<ArticleDocument>(f => f.SupplierDescription),
                        Query = request.Query,
                        Boost = 3.0,
                        Fuzziness = Fuzziness.Auto,
                        Operator = Operator.Or
                    },
                    // Точное совпадение по SupplierDescription (keyword)
                    new MatchQuery
                    {
                        Field = Infer.Field<ArticleDocument>(f => f.SupplierDescription.Suffix("keyword")),
                        Query = request.Query,
                        Boost = 2.0,
                        Operator = Operator.And
                    }
                },
                MinimumShouldMatch = 1
            });
        }

        // Фильтр по SupplierId
        if (request.SupplierId.HasValue)
        {
            queries.Add(new TermQuery
            {
                Field = Infer.Field<ArticleDocument>(f => f.SupplierId),
                Value = request.SupplierId.Value
            });
        }

        return queries.Any() 
            ? new BoolQuery { Must = queries } 
            : new MatchAllQuery();
    }

    #endregion
}

