using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using MySqlConnector;
using TecDocApi.Application.Models;
using TecDocApi.Application.Options;

namespace TecDocApi.Application.Services;

/// <summary>
/// Простой сервис поиска изображений в S3/S3Info.
/// </summary>
public class S3ImageService : IS3ImageService
{
    private static readonly SemaphoreSlim DbConnectionSemaphore = new(20, 20);
    private const int DbConnectionTimeoutMs = 5000;

    private readonly IMinioClient _minioClient;
    private readonly ILogger<S3ImageService> _logger;
    private readonly string _bucketName;
    private readonly string _basePath;
    private readonly string _endpointUrl;
    private readonly string? _s3InfoConnectionString;

    public S3ImageService(
        IOptions<S3Options> s3Options,
        IOptions<DatabaseConnectionOptions> dbOptions,
        ILogger<S3ImageService> logger)
    {
        _logger = logger;

        var s3 = s3Options.Value;
        _endpointUrl = s3.EndpointUrl;
        _bucketName = s3.BucketName;
        _basePath = s3.BasePath;
        _s3InfoConnectionString = dbOptions.Value.S3InfoDatabase;

        var endpoint = new Uri(_endpointUrl);
        _minioClient = new MinioClient()
            .WithEndpoint(endpoint.Host, endpoint.Port)
            .WithCredentials(s3.AccessKey, s3.SecretKey)
            .WithSSL(endpoint.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase))
            .Build();

        _logger.LogInformation(
            "S3ImageService initialized: Endpoint={Endpoint}, Bucket={Bucket}, BasePath={BasePath}",
            _endpointUrl,
            _bucketName,
            _basePath);
    }

    public async Task<IReadOnlyList<S3ImageSearchResult>> SearchImagesByArticleAsync(
        string articleNumber,
        ushort? supplierId = null,
        int maxResults = 20)
    {
        var normalizedArticle = NormalizeForSearch(articleNumber);
        if (string.IsNullOrWhiteSpace(normalizedArticle) || string.IsNullOrWhiteSpace(_s3InfoConnectionString))
        {
            return [];
        }

        maxResults = Math.Clamp(maxResults, 1, 100);

        var tokens = ExtractSearchTokens(articleNumber);
        if (tokens.Count == 0)
        {
            tokens.Add(normalizedArticle);
        }

        var candidates = await LoadS3SearchCandidatesAsync(normalizedArticle, tokens, supplierId);
        if (candidates.Count == 0)
        {
            return [];
        }

        var rankedCandidates = candidates
            .Select(candidate => new
            {
                Candidate = candidate,
                Score = ScoreCandidate(candidate, normalizedArticle, tokens, supplierId)
            })
            .Where(item => item.Score > 0)
            .OrderByDescending(item => item.Score)
            .ThenBy(item => item.Candidate.FileName.Length)
            .ToList();

        var results = new List<S3ImageSearchResult>(maxResults);
        var seenObjectKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var item in rankedCandidates)
        {
            if (results.Count >= maxResults)
            {
                break;
            }

            if (!await ObjectExistsAsync(item.Candidate.ObjectKey))
            {
                continue;
            }

            if (!seenObjectKeys.Add(item.Candidate.ObjectKey))
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
                S3Url = publicUrl
            });
        }

        return results;
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
            .ToList();
    }

    private async Task<bool> ObjectExistsAsync(string objectName)
    {
        try
        {
            await _minioClient.StatObjectAsync(new StatObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(objectName));
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "S3 object does not exist: {ObjectName}", objectName);
            return false;
        }
    }

    private async Task<List<S3SearchCandidate>> LoadS3SearchCandidatesAsync(string normalizedArticle, List<string> tokens, ushort? supplierId)
    {
        var effectiveTokens = tokens
            .Where(token => token.Length >= 2)
            .Distinct(StringComparer.Ordinal)
            .ToList();

        if (effectiveTokens.Count == 0 && !string.IsNullOrWhiteSpace(normalizedArticle))
        {
            effectiveTokens.Add(normalizedArticle);
        }

        if (!await DbConnectionSemaphore.WaitAsync(DbConnectionTimeoutMs))
        {
            _logger.LogWarning("Timeout waiting for S3Info database connection slot");
            return [];
        }

        try
        {
            await using var connection = new MySqlConnection(_s3InfoConnectionString);
            await connection.OpenAsync();

            var folderPaths = await LoadFolderPathsAsync(connection);
            var fileRows = await LoadCandidateFileRowsAsync(connection, normalizedArticle, effectiveTokens);

            var results = new List<S3SearchCandidate>(fileRows.Count);
            foreach (var fileRow in fileRows)
            {
                var extensionSuffix = string.IsNullOrWhiteSpace(fileRow.Extension)
                    ? string.Empty
                    : $".{fileRow.Extension}";
                var fileName = fileRow.Filename + extensionSuffix;

                string? folderPath = null;
                if (fileRow.FolderId.HasValue)
                {
                    folderPaths.TryGetValue(fileRow.FolderId.Value, out folderPath);
                }

                var objectKey = string.IsNullOrWhiteSpace(folderPath)
                    ? fileName
                    : $"{folderPath}/{fileName}";

                if (supplierId.HasValue && !MatchesSupplier(folderPath, fileRow.Filename, supplierId.Value))
                {
                    continue;
                }

                results.Add(new S3SearchCandidate
                {
                    ObjectKey = objectKey,
                    FileName = fileName,
                    FolderPath = folderPath ?? string.Empty,
                    NormalizedFileName = NormalizeForSearch(fileName),
                    NormalizedObjectKey = NormalizeForSearch(objectKey),
                    SupplierId = TryExtractSupplierId(objectKey, supplierId)
                });

                if (results.Count >= 200)
                {
                    break;
                }
            }

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load S3 candidates");
            return [];
        }
        finally
        {
            DbConnectionSemaphore.Release();
        }
    }

    private async Task<List<S3FileRow>> LoadCandidateFileRowsAsync(
        MySqlConnection connection,
        string normalizedArticle,
        List<string> effectiveTokens)
    {
        var results = new Dictionary<long, S3FileRow>();

        var fullTextTokens = effectiveTokens
            .Where(token => token.Length >= 3)
            .Distinct(StringComparer.Ordinal)
            .ToList();

        if (fullTextTokens.Count > 0)
        {
            var fullTextQuery = string.Join(' ', fullTextTokens.Select(token => $"+{token}*"));
            var fullTextSql = """
                              SELECT
                                  f.id,
                                  f.filename,
                                  f.folder_id,
                                  COALESCE(e.extension, '') AS extension,
                                  f.last_modified,
                                  f.created_at
                              FROM Files f
                              LEFT JOIN FileExtensions e ON e.id = f.extension_id
                              WHERE MATCH(f.filename) AGAINST(@fullTextQuery IN BOOLEAN MODE)
                                AND LOWER(COALESCE(e.extension, '')) IN ('jpg', 'jpeg', 'png', 'bmp', 'gif', 'webp', 'tif', 'tiff', 'avif')
                              ORDER BY f.last_modified DESC, f.created_at DESC, f.id DESC
                              LIMIT 400;
                              """;

            await using var fullTextCommand = new MySqlCommand(fullTextSql, connection)
            {
                CommandTimeout = 10
            };
            fullTextCommand.Parameters.AddWithValue("@fullTextQuery", fullTextQuery);

            await using var fullTextReader = await fullTextCommand.ExecuteReaderAsync();
            while (await fullTextReader.ReadAsync())
            {
                var row = ReadFileRow(fullTextReader);
                results[row.Id] = row;
            }
        }

        if (results.Count < 200)
        {
            var likeConditions = effectiveTokens
                .Select((_, index) => $"f.filename LIKE @token{index}")
                .ToList();

            if (likeConditions.Count == 0)
            {
                likeConditions.Add("f.filename LIKE @normalizedArticle");
            }

            var likeSql = $"""
                           SELECT
                               f.id,
                               f.filename,
                               f.folder_id,
                               COALESCE(e.extension, '') AS extension,
                               f.last_modified,
                               f.created_at
                           FROM Files f
                           LEFT JOIN FileExtensions e ON e.id = f.extension_id
                           WHERE {string.Join(" AND ", likeConditions)}
                             AND LOWER(COALESCE(e.extension, '')) IN ('jpg', 'jpeg', 'png', 'bmp', 'gif', 'webp', 'tif', 'tiff', 'avif')
                           ORDER BY f.last_modified DESC, f.created_at DESC, f.id DESC
                           LIMIT 400;
                           """;

            await using var likeCommand = new MySqlCommand(likeSql, connection)
            {
                CommandTimeout = 10
            };

            for (var index = 0; index < effectiveTokens.Count; index++)
            {
                likeCommand.Parameters.AddWithValue($"@token{index}", $"%{effectiveTokens[index]}%");
            }

            if (effectiveTokens.Count == 0)
            {
                likeCommand.Parameters.AddWithValue("@normalizedArticle", $"%{normalizedArticle}%");
            }

            await using var likeReader = await likeCommand.ExecuteReaderAsync();
            while (await likeReader.ReadAsync())
            {
                var row = ReadFileRow(likeReader);
                results[row.Id] = row;
            }
        }

        return results.Values
            .OrderByDescending(row => row.LastModified)
            .ThenByDescending(row => row.CreatedAt)
            .ThenByDescending(row => row.Id)
            .Take(500)
            .ToList();
    }

    private static S3FileRow ReadFileRow(MySqlDataReader reader)
    {
        var idOrdinal = reader.GetOrdinal("id");
        var filenameOrdinal = reader.GetOrdinal("filename");
        var folderIdOrdinal = reader.GetOrdinal("folder_id");
        var extensionOrdinal = reader.GetOrdinal("extension");
        var lastModifiedOrdinal = reader.GetOrdinal("last_modified");
        var createdAtOrdinal = reader.GetOrdinal("created_at");

        return new S3FileRow
        {
            Id = reader.GetInt64(idOrdinal),
            Filename = reader.GetString(filenameOrdinal),
            FolderId = reader.IsDBNull(folderIdOrdinal) ? null : reader.GetInt32(folderIdOrdinal),
            Extension = reader.GetString(extensionOrdinal),
            LastModified = reader.IsDBNull(lastModifiedOrdinal) ? DateTime.MinValue : reader.GetDateTime(lastModifiedOrdinal),
            CreatedAt = reader.IsDBNull(createdAtOrdinal) ? DateTime.MinValue : reader.GetDateTime(createdAtOrdinal)
        };
    }

    private static async Task<Dictionary<int, string>> LoadFolderPathsAsync(MySqlConnection connection)
    {
        const string sql = "SELECT id, folder_name, parent_id FROM Folders;";
        var folders = new Dictionary<int, FolderRow>();

        await using var command = new MySqlCommand(sql, connection)
        {
            CommandTimeout = 10
        };

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var idOrdinal = reader.GetOrdinal("id");
            var folderNameOrdinal = reader.GetOrdinal("folder_name");
            var parentIdOrdinal = reader.GetOrdinal("parent_id");

            var folder = new FolderRow
            {
                Id = reader.GetInt32(idOrdinal),
                FolderName = reader.GetString(folderNameOrdinal),
                ParentId = reader.IsDBNull(parentIdOrdinal) ? null : reader.GetInt32(parentIdOrdinal)
            };

            folders[folder.Id] = folder;
        }

        var result = new Dictionary<int, string>();
        foreach (var folderId in folders.Keys)
        {
            result[folderId] = BuildFolderPath(folderId, folders, result);
        }

        return result;
    }

    private static string BuildFolderPath(int folderId, IReadOnlyDictionary<int, FolderRow> folders, IDictionary<int, string> cache)
    {
        if (cache.TryGetValue(folderId, out var cachedPath))
        {
            return cachedPath;
        }

        if (!folders.TryGetValue(folderId, out var folder))
        {
            return string.Empty;
        }

        var parentPath = folder.ParentId.HasValue
            ? BuildFolderPath(folder.ParentId.Value, folders, cache)
            : string.Empty;

        var fullPath = string.IsNullOrWhiteSpace(parentPath)
            ? folder.FolderName
            : $"{parentPath}/{folder.FolderName}";

        cache[folderId] = fullPath;
        return fullPath;
    }

    private static bool MatchesSupplier(string? folderPath, string fileNameWithoutExtension, ushort supplierId)
    {
        var supplierText = supplierId.ToString();

        if (!string.IsNullOrWhiteSpace(folderPath))
        {
            var parts = folderPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Any(part => string.Equals(part, supplierText, StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }
        }

        return fileNameWithoutExtension.StartsWith($"{supplierText}_", StringComparison.OrdinalIgnoreCase);
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
}

