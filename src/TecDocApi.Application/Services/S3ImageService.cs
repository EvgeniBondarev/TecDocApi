using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Minio;
using Minio.DataModel.Args;
using MySqlConnector;
using TecDocApi.Application.Models;

namespace TecDocApi.Application.Services;

/// <summary>
/// Сервис для работы с изображениями в S3 хранилище Timeweb
/// </summary>
public class S3ImageService : IS3ImageService
{
    private static readonly HashSet<string> ImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".bmp", ".gif", ".webp", ".tif", ".tiff", ".avif"
    };

    // Семафор для ограничения одновременных подключений к S3Info БД
    // Предотвращает исчерпание лимита подключений MySQL (по умолчанию 151)
    private static readonly SemaphoreSlim DbConnectionSemaphore = new SemaphoreSlim(20, 20);
    private const int DbConnectionTimeoutMs = 5000; // 5 секунд таймаут для получения слота

    private readonly IMinioClient _minioClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<S3ImageService> _logger;
    private readonly string _bucketName;
    private readonly string _basePath;
    private readonly string _endpointUrl;
    private readonly string? _s3InfoConnectionString;
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromHours(12);

    private sealed class RankedS3Candidate
    {
        public required S3SearchCandidate Candidate { get; init; }

        public required int Score { get; init; }
    }

    public S3ImageService(
        IConfiguration configuration,
        IMemoryCache cache,
        ILogger<S3ImageService> logger)
    {
        _cache = cache;
        _logger = logger;

        var accessKey = configuration["S3:AccessKey"] ?? throw new InvalidOperationException("S3:AccessKey не настроен");
        var secretKey = configuration["S3:SecretKey"] ?? throw new InvalidOperationException("S3:SecretKey не настроен");
        _endpointUrl = configuration["S3:EndpointUrl"] ?? throw new InvalidOperationException("S3:EndpointUrl не настроен");
        _bucketName = configuration["S3:BucketName"] ?? throw new InvalidOperationException("S3:BucketName не настроен");
        _basePath = configuration["S3:BasePath"] ?? "TD2018/images";
        _s3InfoConnectionString = ResolveS3InfoConnectionString(configuration);
        var endpoint = new Uri(_endpointUrl);

        _minioClient = new MinioClient()
            .WithEndpoint(endpoint.Host, endpoint.Port)
            .WithCredentials(accessKey, secretKey)
            .WithSSL(endpoint.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase))
            .Build();

        _logger.LogInformation("Инициализация MinIO клиента: Endpoint={Endpoint}, Bucket={Bucket}, BasePath={BasePath}",
            _endpointUrl, _bucketName, _basePath);
    }

    /// <summary>
    /// Получает URL изображения из S3 (с кэшированием)
    /// </summary>
    /// <remarks>
    /// Формирует прямой публичный URL к изображению в S3 хранилище Timeweb.
    /// Аналогично методу make_url в Python версии.
    /// </remarks>
    public Task<string?> GetImageUrlAsync(ushort supplierId, string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return Task.FromResult<string?>(null);
        }

        var cacheKey = $"s3_image_url_{supplierId}_{fileName}";

        // Проверяем кэш
        if (_cache.TryGetValue(cacheKey, out string? cachedUrl))
        {
            return Task.FromResult(cachedUrl);
        }

        try
        {
            var imageUrl = $"/api/Images/{supplierId}/{Uri.EscapeDataString(fileName)}/stream";

            // Кэшируем URL с указанием размера (если SizeLimit установлен)
            try
            {
                var cacheEntryOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = _cacheExpiration,
                    Size = 1 // Минимальный размер для кэша с SizeLimit
                };
                _cache.Set(cacheKey, imageUrl, cacheEntryOptions);
            }
            catch (Exception cacheEx)
            {
                // Если кэширование не удалось, просто логируем и продолжаем
                _logger.LogWarning(cacheEx, "Не удалось закэшировать URL изображения: SupplierId={SupplierId}, FileName={FileName}", supplierId, fileName);
            }

            return Task.FromResult<string?>(imageUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении URL изображения: SupplierId={SupplierId}, FileName={FileName}", supplierId, fileName);
            return Task.FromResult<string?>(null);
        }
    }

    private string BuildObjectName(ushort supplierId, string fileName)
    {
        return $"{_basePath}/{supplierId}/{fileName}";
    }

    private string BuildPublicObjectUrl(string objectKey)
    {
        var endpoint = _endpointUrl.TrimEnd('/');
        var encodedKey = string.Join('/', objectKey
            .Split('/', StringSplitOptions.RemoveEmptyEntries)
            .Select(Uri.EscapeDataString));

        return $"{endpoint}/{_bucketName}/{encodedKey}";
    }

    private static string NormalizeForSearch(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var builder = new StringBuilder(value.Length);
        foreach (var character in value)
        {
            if (char.IsLetterOrDigit(character))
            {
                builder.Append(char.ToUpperInvariant(character));
            }
        }

        return builder.ToString();
    }

    private static List<string> ExtractSearchTokens(string value)
    {
        return Regex.Split(value.ToUpperInvariant(), "[^A-Z0-9]+")
            .Where(token => !string.IsNullOrWhiteSpace(token))
            .Distinct(StringComparer.Ordinal)
            .OrderByDescending(token => token.Length)
            .ToList();
    }

    private static string? ResolveS3InfoConnectionString(IConfiguration configuration)
    {
        var directConnectionString = Environment.GetEnvironmentVariable("S3INFO_DATABASE_CONNECTION_STRING")
            ?? configuration.GetConnectionString("S3InfoDatabase");

        if (!string.IsNullOrWhiteSpace(directConnectionString))
        {
            return directConnectionString.Trim().Trim('\'').Trim('"');
        }

        var tecDocConnectionString = Environment.GetEnvironmentVariable("TECDOC_DATABASE_CONNECTION_STRING")
            ?? configuration.GetConnectionString("TecDocDatabase");

        if (string.IsNullOrWhiteSpace(tecDocConnectionString))
        {
            return null;
        }

        var builder = new MySqlConnectionStringBuilder(tecDocConnectionString)
        {
            Database = "S3Info"
        };

        return builder.ConnectionString;
    }

    private async Task<string?> ResolveObjectNameAsync(ushort supplierId, string fileName)
    {
        var cacheKey = $"s3_object_name_{supplierId}_{fileName}";
        if (_cache.TryGetValue(cacheKey, out string? cachedObjectName))
        {
            return cachedObjectName;
        }

        var legacyObjectName = BuildObjectName(supplierId, fileName);
        if (await ObjectExistsAsync(legacyObjectName))
        {
            CacheResolvedObjectName(cacheKey, legacyObjectName);
            return legacyObjectName;
        }

        var indexedObjectName = await FindObjectNameInS3InfoAsync(fileName, supplierId);
        if (!string.IsNullOrWhiteSpace(indexedObjectName) && await ObjectExistsAsync(indexedObjectName))
        {
            CacheResolvedObjectName(cacheKey, indexedObjectName);
            return indexedObjectName;
        }

        return null;
    }

    private void CacheResolvedObjectName(string cacheKey, string objectName)
    {
        try
        {
            _cache.Set(cacheKey, objectName, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1),
                Size = 1
            });
        }
        catch (Exception cacheEx)
        {
            _logger.LogWarning(cacheEx, "Не удалось закэшировать S3 object key {ObjectName}", objectName);
        }
    }

    private async Task<bool> ObjectExistsAsync(string objectName)
    {
        var cacheKey = $"s3_object_exists_{objectName}";
        if (_cache.TryGetValue(cacheKey, out bool cachedExists))
        {
            return cachedExists;
        }

        try
        {
            await _minioClient.StatObjectAsync(new StatObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(objectName));

            try
            {
                _cache.Set(cacheKey, true, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1),
                    Size = 1
                });
            }
            catch
            {
                // ignored
            }

            return true;
        }
        catch
        {
            try
            {
                _cache.Set(cacheKey, false, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
                    Size = 1
                });
            }
            catch
            {
                // ignored
            }

            return false;
        }
    }

    private async Task<string?> FindObjectNameInS3InfoAsync(string fileName, ushort? supplierId = null)
    {
        var matches = await SearchImagesByArticleAsync(fileName, supplierId, 1);
        return matches.FirstOrDefault()?.ObjectKey;
    }

    public async Task<IReadOnlyList<S3ImageSearchResult>> SearchImagesByArticleAsync(string articleNumber, ushort? supplierId = null, int maxResults = 20)
    {
        var normalizedArticle = NormalizeForSearch(articleNumber);
        if (string.IsNullOrWhiteSpace(_s3InfoConnectionString) || string.IsNullOrWhiteSpace(normalizedArticle))
        {
            return Array.Empty<S3ImageSearchResult>();
        }

        maxResults = Math.Clamp(maxResults, 1, 100);
        var cacheKey = $"s3_article_search_{supplierId?.ToString() ?? "all"}_{normalizedArticle}_{maxResults}";
        if (_cache.TryGetValue(cacheKey, out IReadOnlyList<S3ImageSearchResult>? cachedResults) && cachedResults != null)
        {
            return cachedResults;
        }

        var tokens = ExtractSearchTokens(articleNumber);
        if (tokens.Count == 0)
        {
            tokens.Add(normalizedArticle);
        }

        var candidates = await LoadS3SearchCandidatesAsync(normalizedArticle, tokens, supplierId);
        if (candidates.Count == 0)
        {
            return Array.Empty<S3ImageSearchResult>();
        }

        var rankedCandidates = candidates
            .Select(candidate => new RankedS3Candidate
            {
                Candidate = candidate,
                Score = ScoreCandidate(candidate, normalizedArticle, tokens, supplierId)
            })
            .Where(item => item.Score > 0)
            .OrderByDescending(item => item.Score)
            .ThenBy(item => item.Candidate.FileName.Length)
            .Take(Math.Min(24, Math.Max(maxResults * 4, 8)))
            .ToList();

        var results = await MaterializeTopExistingResultsAsync(rankedCandidates, normalizedArticle, maxResults);

        try
        {
            _cache.Set(cacheKey, results, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(20),
                Size = 1
            });
        }
        catch (Exception cacheEx)
        {
            _logger.LogWarning(cacheEx, "Не удалось закэшировать результаты поиска картинок по артикулу {ArticleNumber}", articleNumber);
        }

        return results;
    }

    private async Task<List<S3ImageSearchResult>> MaterializeTopExistingResultsAsync(
        List<RankedS3Candidate> rankedCandidates,
        string normalizedArticle,
        int maxResults)
    {
        var results = new List<S3ImageSearchResult>(maxResults);
        var seenObjectKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var batchSize = Math.Min(Math.Max(maxResults, 2), 6);

        for (var offset = 0; offset < rankedCandidates.Count && results.Count < maxResults; offset += batchSize)
        {
            var batch = rankedCandidates.Skip(offset).Take(batchSize).ToList();
            var existenceChecks = await Task.WhenAll(batch.Select(async item => new
            {
                item.Candidate,
                item.Score,
                Exists = await ObjectExistsAsync(item.Candidate.ObjectKey)
            }));

            foreach (var item in existenceChecks)
            {
                if (!item.Exists || !seenObjectKeys.Add(item.Candidate.ObjectKey))
                {
                    continue;
                }

                var publicUrl = BuildPublicObjectUrl(item.Candidate.ObjectKey);
                results.Add(new S3ImageSearchResult
                {
                    PictureName = item.Candidate.FileName,
                    ObjectKey = item.Candidate.ObjectKey,
                    SupplierId = item.Candidate.SupplierId,
                    MatchScore = item.Score,
                    MatchedBy = DescribeMatch(item.Candidate, normalizedArticle),
                    Url = publicUrl,
                    StreamUrl = $"/api/Images/by-key/stream?objectKey={Uri.EscapeDataString(item.Candidate.ObjectKey)}",
                    S3Url = publicUrl
                });

                if (results.Count >= maxResults)
                {
                    break;
                }
            }
        }

        return results;
    }

    private async Task<List<S3SearchCandidate>> LoadS3SearchCandidatesAsync(string normalizedArticle, List<string> tokens, ushort? supplierId)
    {
        if (string.IsNullOrWhiteSpace(_s3InfoConnectionString))
        {
            return new List<S3SearchCandidate>();
        }

        var effectiveTokens = tokens
            .Where(token => token.Length >= 2)
            .Distinct(StringComparer.Ordinal)
            .ToList();

        if (effectiveTokens.Count == 0 && !string.IsNullOrWhiteSpace(normalizedArticle))
        {
            effectiveTokens.Add(normalizedArticle);
        }

        var tokenConditions = effectiveTokens
            .Select((_, index) => $"UPPER(CONCAT(f.filename, IFNULL(CONCAT('.', e.extension), ''))) LIKE @token{index}")
            .ToList();

        var sql = $"""
                   WITH RECURSIVE folder_path AS (
                       SELECT id, folder_name, parent_id, folder_name AS full_path
                       FROM Folders
                       WHERE parent_id IS NULL
                       UNION ALL
                       SELECT child.id, child.folder_name, child.parent_id, CONCAT(parent.full_path, '/', child.folder_name) AS full_path
                       FROM Folders child
                       JOIN folder_path parent ON child.parent_id = parent.id
                   )
                   SELECT CASE
                           WHEN fp.full_path IS NULL OR fp.full_path = '' THEN CONCAT(f.filename, IFNULL(CONCAT('.', e.extension), ''))
                           ELSE CONCAT(fp.full_path, '/', f.filename, IFNULL(CONCAT('.', e.extension), ''))
                       END AS file_key,
                       CONCAT(f.filename, IFNULL(CONCAT('.', e.extension), '')) AS file_name,
                       IFNULL(fp.full_path, '') AS folder_path,
                       f.last_modified,
                       f.created_at,
                       f.id
                   FROM Files f
                   LEFT JOIN FileExtensions e ON e.id = f.extension_id
                   LEFT JOIN folder_path fp ON fp.id = f.folder_id
                   WHERE (
                           {string.Join(" AND ", tokenConditions)}
                       )
                       {(supplierId.HasValue ? "AND (fp.full_path LIKE @supplierFolderPattern OR CONCAT(f.filename, IFNULL(CONCAT('.', e.extension), '')) LIKE @supplierFilePattern)" : string.Empty)}
                   ORDER BY f.last_modified DESC, f.created_at DESC, f.id DESC
                   LIMIT 200;
                   """;

        // Ограничиваем одновременные подключения к БД для предотвращения "Too many connections"
        if (!await DbConnectionSemaphore.WaitAsync(DbConnectionTimeoutMs))
        {
            _logger.LogWarning("Timeout ожидания свободного слота для подключения к S3Info БД. Слишком много одновременных запросов.");
            return new List<S3SearchCandidate>();
        }

        try
        {
            await using var connection = new MySqlConnection(_s3InfoConnectionString);
            try
            {
                await connection.OpenAsync();
            }
            catch (MySqlException ex) when (ex.Number == 1040 || ex.Message.Contains("Too many connections"))
            {
                _logger.LogError(ex, "Error: MySQL connection limit reached. Consider increasing max_connections or implementing connection pooling.");
                return new List<S3SearchCandidate>();
            }

            await using var command = new MySqlCommand(sql, connection);
            command.CommandTimeout = 10; // Таймаут для длительных запросов
            
            for (var index = 0; index < effectiveTokens.Count; index++)
            {
                command.Parameters.AddWithValue($"@token{index}", $"%{effectiveTokens[index]}%");
            }

            if (supplierId.HasValue)
            {
                command.Parameters.AddWithValue("@supplierFolderPattern", $"%/{supplierId.Value}/%");
                command.Parameters.AddWithValue("@supplierFilePattern", $"{supplierId.Value}_%");
            }

            var results = new List<S3SearchCandidate>();
            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var objectKey = reader.GetString("file_key");
                var fileName = reader.GetString("file_name");
                if (!IsImageFile(fileName))
                {
                    continue;
                }

                results.Add(new S3SearchCandidate
                {
                    ObjectKey = objectKey,
                    FileName = fileName,
                    FolderPath = reader.GetString("folder_path"),
                    NormalizedFileName = NormalizeForSearch(fileName),
                    NormalizedObjectKey = NormalizeForSearch(objectKey),
                    SupplierId = TryExtractSupplierId(objectKey, supplierId)
                });
            }

            return results;
        }
        finally
        {
            // Освобождаем слот для других ожидающих запросов
            DbConnectionSemaphore.Release();
        }
    }

    private static bool IsImageFile(string fileName)
    {
        var extension = Path.GetExtension(fileName);
        return !string.IsNullOrWhiteSpace(extension) && ImageExtensions.Contains(extension);
    }

    private static ushort? TryExtractSupplierId(string objectKey, ushort? preferredSupplierId)
    {
        if (preferredSupplierId.HasValue)
        {
            return preferredSupplierId.Value;
        }

        var parts = objectKey.Split('/', StringSplitOptions.RemoveEmptyEntries);
        foreach (var part in parts)
        {
            if (ushort.TryParse(part, out var parsed))
            {
                return parsed;
            }
        }

        return null;
    }

    private static int ScoreCandidate(S3SearchCandidate candidate, string normalizedArticle, List<string> tokens, ushort? supplierId)
    {
        var score = 0;
        if (string.Equals(candidate.NormalizedFileName, normalizedArticle, StringComparison.Ordinal))
        {
            score += 1000;
        }

        if (candidate.NormalizedFileName.StartsWith(normalizedArticle, StringComparison.Ordinal))
        {
            score += 500;
        }

        if (candidate.NormalizedFileName.Contains(normalizedArticle, StringComparison.Ordinal))
        {
            score += 350;
        }

        if (candidate.NormalizedObjectKey.Contains(normalizedArticle, StringComparison.Ordinal))
        {
            score += 200;
        }

        foreach (var token in tokens)
        {
            if (token.Length < 3)
            {
                continue;
            }

            if (candidate.NormalizedFileName.Contains(token, StringComparison.Ordinal))
            {
                score += 50;
            }
            else if (candidate.NormalizedObjectKey.Contains(token, StringComparison.Ordinal))
            {
                score += 20;
            }
        }

        if (supplierId.HasValue && candidate.SupplierId == supplierId.Value)
        {
            score += 300;
        }

        score -= Math.Abs(candidate.NormalizedFileName.Length - normalizedArticle.Length);
        return score;
    }

    private static string DescribeMatch(S3SearchCandidate candidate, string normalizedArticle)
    {
        if (string.Equals(candidate.NormalizedFileName, normalizedArticle, StringComparison.Ordinal))
        {
            return "exact-normalized-file";
        }

        if (candidate.NormalizedFileName.StartsWith(normalizedArticle, StringComparison.Ordinal))
        {
            return "prefix-normalized-file";
        }

        if (candidate.NormalizedFileName.Contains(normalizedArticle, StringComparison.Ordinal))
        {
            return "contains-normalized-file";
        }

        return "contains-normalized-path";
    }

    public async Task<Stream?> GetImageStreamByObjectKeyAsync(string objectKey)
    {
        if (string.IsNullOrWhiteSpace(objectKey))
        {
            return null;
        }

        try
        {
            var memoryStream = new MemoryStream();
            await _minioClient.GetObjectAsync(new GetObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(objectKey)
                .WithCallbackStream(async void (stream) => await stream.CopyToAsync(memoryStream)));

            memoryStream.Position = 0;
            return memoryStream;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении изображения из S3 по object key {ObjectKey}", objectKey);
            return null;
        }
    }

    /// <summary>
    /// Получает изображение как поток данных из S3
    /// </summary>
    public async Task<Stream?> GetImageStreamAsync(ushort supplierId, string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return null;
        }

        try
        {
            var objectName = await ResolveObjectNameAsync(supplierId, fileName);
            if (string.IsNullOrWhiteSpace(objectName))
            {
                return null;
            }

            var memoryStream = new MemoryStream();
            await _minioClient.GetObjectAsync(new GetObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(objectName)
                .WithCallbackStream(async void (stream) => await stream.CopyToAsync(memoryStream)));

            memoryStream.Position = 0;
            return memoryStream;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении изображения из S3: SupplierId={SupplierId}, FileName={FileName}", supplierId, fileName);
            return null;
        }
    }

    /// <summary>
    /// Проверяет существование изображения в S3 (с кэшированием)
    /// </summary>
    /// <remarks>
    /// Использует head_object (GetObjectMetadata) для проверки существования файла.
    /// Аналогично Python версии: s3_client.head_object(Bucket=..., Key=...)
    /// </remarks>
    /// <summary>
    /// Проверяет существование изображения в S3 и возвращает URL за один резолв object name.
    /// Избегает двойного S3-запроса по сравнению с последовательным вызовом ImageExistsAsync + GetImageUrlAsync.
    /// </summary>
    public async Task<string?> TryGetImageUrlAsync(ushort supplierId, string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return null;

        // Если URL уже закэширован — значит объект точно существовал, возвращаем сразу
        var urlCacheKey = $"s3_image_url_{supplierId}_{fileName}";
        if (_cache.TryGetValue(urlCacheKey, out string? cachedUrl))
            return cachedUrl;

        // Один резолв вместо двух (ImageExistsAsync → ResolveObjectNameAsync, GetImageUrlAsync)
        var objectName = await ResolveObjectNameAsync(supplierId, fileName);
        if (string.IsNullOrWhiteSpace(objectName))
            return null;

        var imageUrl = $"/api/Images/{supplierId}/{Uri.EscapeDataString(fileName)}/stream";

        try
        {
            _cache.Set(urlCacheKey, imageUrl, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = _cacheExpiration,
                Size = 1
            });
            // также кэшируем exists-флаг, чтобы повторные ImageExistsAsync не лезли в S3
            var existsCacheKey = $"s3_image_exists_{supplierId}_{fileName}";
            _cache.Set(existsCacheKey, true, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1),
                Size = 1
            });
        }
        catch
        {
            // ignored
        }

        return imageUrl;
    }

    public async Task<bool> ImageExistsAsync(ushort supplierId, string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return false;
        }

        var cacheKey = $"s3_image_exists_{supplierId}_{fileName}";

        // Проверяем кэш
        if (_cache.TryGetValue(cacheKey, out bool cachedExists))
        {
            return cachedExists;
        }

        try
        {
            var objectName = await ResolveObjectNameAsync(supplierId, fileName);
            if (string.IsNullOrWhiteSpace(objectName))
            {
                return false;
            }

            // Кэшируем результат (кэш на меньшее время для проверок существования)
            try
            {
                var cacheEntryOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1),
                    Size = 1 // Минимальный размер для кэша с SizeLimit
                };
                _cache.Set(cacheKey, true, cacheEntryOptions);
            }
            catch (Exception cacheEx)
            {
                _logger.LogWarning(cacheEx, "Не удалось закэшировать результат проверки существования изображения");
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при проверке существования изображения: SupplierId={SupplierId}, FileName={FileName}, Endpoint={Endpoint}", 
                supplierId, fileName, _endpointUrl);
            return false;
        }
    }
}

internal sealed class S3SearchCandidate
{
    public string ObjectKey { get; set; } = string.Empty;

    public string FileName { get; set; } = string.Empty;

    public string FolderPath { get; set; } = string.Empty;

    public string NormalizedFileName { get; set; } = string.Empty;

    public string NormalizedObjectKey { get; set; } = string.Empty;

    public ushort? SupplierId { get; set; }
}

