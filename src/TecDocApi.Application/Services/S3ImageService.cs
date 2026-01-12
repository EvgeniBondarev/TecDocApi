using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace TecDocApi.Application.Services;

/// <summary>
/// Сервис для работы с изображениями в S3 хранилище Timeweb
/// </summary>
public class S3ImageService : IS3ImageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly IMemoryCache _cache;
    private readonly ILogger<S3ImageService> _logger;
    private readonly string _bucketName;
    private readonly string _basePath;
    private readonly string _endpointUrl;
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromHours(24); // Кэш URL на 24 часа

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
        var regionName = configuration["S3:RegionName"] ?? "ru-1";
        _bucketName = configuration["S3:BucketName"] ?? throw new InvalidOperationException("S3:BucketName не настроен");
        _basePath = configuration["S3:BasePath"] ?? "TD2018/images";

        // Логируем конфигурацию для отладки
        _logger.LogInformation("Инициализация S3 клиента: Endpoint={Endpoint}, Bucket={Bucket}, BasePath={BasePath}, Region={Region}", 
            _endpointUrl, _bucketName, _basePath, regionName);

        // Создаем S3 клиент с кастомным endpoint для Timeweb
        // Конфигурация аналогична Python версии: endpoint_url и region_name
        var config = new AmazonS3Config
        {
            // Используем кастомный endpoint напрямую (как endpoint_url в boto3)
            ServiceURL = _endpointUrl,
            // Используем регион как есть (как region_name в boto3)
            // Для кастомных endpoint используем USEast1 как заглушку, так как "ru-1" не существует в AWS
            RegionEndpoint = RegionEndpoint.USEast1,
            // ForcePathStyle = true для кастомных endpoint (path-style: https://endpoint/bucket/key)
            // Это необходимо для S3-совместимых хранилищ, которые не поддерживают virtual-hosted style
            ForcePathStyle = true
        };

        // Создаем клиент с явным указанием конфигурации
        // Аналогично: Session(aws_access_key_id, aws_secret_access_key).client('s3', endpoint_url=..., region_name=...)
        _s3Client = new AmazonS3Client(accessKey, secretKey, config);
        
        _logger.LogInformation("S3 клиент успешно инициализирован с endpoint: {Endpoint}", _endpointUrl);
    }

    /// <summary>
    /// Получает URL изображения из S3 (с кэшированием)
    /// </summary>
    /// <remarks>
    /// Формирует прямой публичный URL к изображению в S3 хранилище Timeweb.
    /// Аналогично методу make_url в Python версии.
    /// </remarks>
    public async Task<string?> GetImageUrlAsync(ushort supplierId, string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return null;
        }

        var cacheKey = $"s3_image_url_{supplierId}_{fileName}";

        // Проверяем кэш
        if (_cache.TryGetValue(cacheKey, out string? cachedUrl))
        {
            return cachedUrl;
        }

        try
        {
            // Формируем прямой URL к изображению (аналогично Python: make_url)
            // Формат: https://s3.timeweb.cloud/{bucket}/{basePath}/{supplierId}/{fileName}
            // Не проверяем существование файла для максимальной производительности
            // Если файл не существует, браузер получит 404 при загрузке
            var imageUrl = $"{_endpointUrl}/{_bucketName}/{_basePath}/{supplierId}/{fileName}";

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

            return imageUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении URL изображения: SupplierId={SupplierId}, FileName={FileName}", supplierId, fileName);
            // Даже при ошибке формируем URL, так как он может быть валидным
            return $"{_endpointUrl}/{_bucketName}/{_basePath}/{supplierId}/{fileName}";
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
            var key = $"{_basePath}/{supplierId}/{fileName}";

            var request = new GetObjectRequest
            {
                BucketName = _bucketName,
                Key = key
            };

            var response = await _s3Client.GetObjectAsync(request);

            if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
            {
                // Копируем поток в MemoryStream для безопасного использования
                // Это необходимо, так как ResponseStream будет закрыт после завершения запроса
                var memoryStream = new MemoryStream();
                await response.ResponseStream.CopyToAsync(memoryStream);
                memoryStream.Position = 0;
                return memoryStream;
            }

            return null;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogDebug("Изображение не найдено в S3: SupplierId={SupplierId}, FileName={FileName}", supplierId, fileName);
            return null;
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
            var key = $"{_basePath}/{supplierId}/{fileName}";

            _logger.LogDebug("Проверка существования изображения в S3: Bucket={Bucket}, Key={Key}", _bucketName, key);

            // Используем head_object для проверки существования (аналогично Python: head_object)
            var request = new GetObjectMetadataRequest
            {
                BucketName = _bucketName,
                Key = key
            };

            await _s3Client.GetObjectMetadataAsync(request);

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
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogDebug("Изображение не найдено в S3: SupplierId={SupplierId}, FileName={FileName}", supplierId, fileName);
            // Кэшируем отрицательный результат на меньшее время
            try
            {
                var cacheEntryOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30),
                    Size = 1 // Минимальный размер для кэша с SizeLimit
                };
                _cache.Set(cacheKey, false, cacheEntryOptions);
            }
            catch (Exception cacheEx)
            {
                _logger.LogWarning(cacheEx, "Не удалось закэшировать отрицательный результат проверки");
            }
            return false;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.Forbidden)
        {
            // Если получили Forbidden, возможно проблема с подписью запроса для кастомного endpoint
            // В этом случае считаем, что файл может существовать, но доступ ограничен
            // Возвращаем true, чтобы URL был сформирован (браузер сам проверит доступность)
            _logger.LogWarning("Получен Forbidden при проверке существования изображения. Возможно проблема с подписью запроса для кастомного endpoint. SupplierId={SupplierId}, FileName={FileName}", 
                supplierId, fileName);
            // Не кэшируем результат при Forbidden, чтобы можно было повторить попытку
            return true; // Возвращаем true, чтобы URL был сформирован
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, "Ошибка S3 при проверке существования изображения: SupplierId={SupplierId}, FileName={FileName}, StatusCode={StatusCode}, Endpoint={Endpoint}", 
                supplierId, fileName, ex.StatusCode, _endpointUrl);
            // При других ошибках возвращаем true, чтобы URL был сформирован
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при проверке существования изображения: SupplierId={SupplierId}, FileName={FileName}, Endpoint={Endpoint}", 
                supplierId, fileName, _endpointUrl);
            // При неизвестных ошибках возвращаем true, чтобы URL был сформирован
            return true;
        }
    }
}

