using System.IO.Compression;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.OpenApi.Models;
using TecDocApi.API.Filters;
using TecDocApi.Application.Services;
using TecDocApi.Infrastructure.Extensions;

namespace TecDocApi.API.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Добавляет все сервисы приложения
    /// </summary>
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                options.JsonSerializerOptions.WriteIndented = false;
                options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                options.JsonSerializerOptions.TypeInfoResolver = JsonSerializer.IsReflectionEnabledByDefault 
                    ? new DefaultJsonTypeInfoResolver() 
                    : null;
            });

        services.AddSwagger();
        services.AddCaching();
        services.AddCompression();
        services.AddRateLimiting();
        services.AddSecurity();
        services.AddApplicationServices(configuration);

        return services;
    }

    /// <summary>
    /// Добавляет Swagger/OpenAPI с XML документацией
    /// </summary>
    private static IServiceCollection AddSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "TecDoc API",
                Version = "v1",
                Description = """
                    ### 📦 Каталог автозапчастей TecDoc
                    
                    Современное REST API для работы с каталогом автозапчастей TecDoc с поддержкой быстрого полнотекстового поиска через Elasticsearch.
                    
                    #### 🔍 Возможности поиска:
                    - **Elasticsearch поиск** - быстрый полнотекстовый поиск по артикулам и поставщикам
                    - **Классический поиск** - поиск напрямую в MySQL с полной информацией
                    - **Ngram поиск** - поиск по части артикула (например, "123" найдет "12345")
                    - **Русский язык** - поддержка морфологии и стемминга для русского языка
                    
                    #### 📊 Эндпоинты:
                    - `/api/ArticleSearch/*` - поиск артикулов через Elasticsearch
                    - `/api/SupplierSearch/*` - поиск поставщиков через Elasticsearch
                    - `/api/Images/article-search` - поиск картинок по articleNumber с опциональным `supplierId`
                    - `/api/Images/s3-search` - глобальный поиск картинок по всем папкам S3
                    - `/api/v1/articles/*` - классический поиск артикулов (MySQL)
                    - `/api/v1/suppliers/*` - классический поиск поставщиков (MySQL)

                    #### 🖼️ Новый поток картинок:
                    - `/api/ArticleSearch/search` возвращает только данные поиска без поля картинок
                    - изображения загружаются вторым запросом через `/api/Images/article-search`
                    - для глобального поиска по S3 используйте `/api/Images/s3-search?articleNumber=ALM2019YX`
                    - в тестовом стенде превью загружаются сразу; если формат не поддерживается браузером, используйте ссылку `Открыть файл` или `Открыть stream`
                    - для ручной проверки используйте `/tests`
                    
                    #### 🚀 Особенности:
                    - Автоматическая синхронизация данных из MySQL в Elasticsearch
                    - Rate limiting для защиты от DDoS
                    - Response caching для повышения производительности
                    - Health checks для мониторинга состояния
                    
                    **Base URL:** `http://localhost:8080` (локально) или `https://api.tecdoc.ru` (production)
                    
                    Все ответы в формате `application/json`
                    
                    Подробные примеры использования см. в файле `API_EXAMPLES.md`
                    """,
                Contact = new OpenApiContact
                {
                    Name = "TecDoc API Support",
                    Email = "support@tecdoc.ru"
                }
            });

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                c.IncludeXmlComments(xmlPath);
            }

            c.TagActionsBy(api => 
            {
                var controllerName = api.ActionDescriptor.RouteValues["controller"] ?? "Default";
                return new[] { controllerName };
            });
            c.DocInclusionPredicate((_, _) => true);
            c.OperationFilter<ResponseExamplesFilter>();
            
            // Примеры запросов и ответов добавляются через атрибуты SwaggerRequestExample и SwaggerResponseExample в контроллерах
        });

        return services;
    }

    /// <summary>
    /// Добавляет кэширование (Response и Memory)
    /// </summary>
    private static IServiceCollection AddCaching(this IServiceCollection services)
    {
        services.AddResponseCaching(options =>
        {
            options.MaximumBodySize = 64 * 1024 * 1024;
            options.SizeLimit = 100 * 1024 * 1024;
            options.UseCaseSensitivePaths = false;
        });

        services.AddMemoryCache(options =>
        {
            options.SizeLimit = 1024;
            options.CompactionPercentage = 0.25;
        });

        return services;
    }

    /// <summary>
    /// Добавляет сжатие ответов (Gzip/Brotli)
    /// </summary>
    private static IServiceCollection AddCompression(this IServiceCollection services)
    {
        services.AddResponseCompression(options =>
        {
            options.EnableForHttps = true;
            options.Providers.Add<BrotliCompressionProvider>();
            options.Providers.Add<GzipCompressionProvider>();
            options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[]
            {
                "application/json",
                "application/xml",
                "text/plain",
                "text/css",
                "application/javascript",
                "text/html",
                "application/xml",
                "text/xml",
                "application/atom+xml",
                "text/json"
            });
        });

        services.Configure<BrotliCompressionProviderOptions>(options =>
        {
            options.Level = CompressionLevel.Optimal;
        });

        services.Configure<GzipCompressionProviderOptions>(options =>
        {
            options.Level = CompressionLevel.Optimal;
        });

        return services;
    }

    /// <summary>
    /// Добавляет Rate Limiting для защиты от DDoS и brute-force
    /// </summary>
    private static IServiceCollection AddRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.AddFixedWindowLimiter("api", opt =>
            {
                opt.PermitLimit = 100;
                opt.Window = TimeSpan.FromSeconds(10);
                opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                opt.QueueLimit = 10;
            });

            options.AddFixedWindowLimiter("search", opt =>
            {
                opt.PermitLimit = 50;
                opt.Window = TimeSpan.FromSeconds(10);
                opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                opt.QueueLimit = 5;
            });

            options.OnRejected = async (context, cancellationToken) =>
            {
                var loggerFactory = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger("RateLimiting");
                var clientIp = context.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                
                logger.LogWarning(
                    "SECURITY: Rate limit exceeded для IP {ClientIp} на {Path}",
                    clientIp,
                    context.HttpContext.Request.Path);

                context.HttpContext.Response.StatusCode = 429;
                context.HttpContext.Response.ContentType = "application/json";
                
                var response = JsonSerializer.Serialize(new
                {
                    error = "Too many requests. Please try again later.",
                    retryAfter = 10
                });

                await context.HttpContext.Response.WriteAsync(response, cancellationToken);
            };
        });

        return services;
    }

    /// <summary>
    /// Добавляет меры безопасности
    /// </summary>
    private static IServiceCollection AddSecurity(this IServiceCollection services)
    {
        services.Configure<FormOptions>(options =>
        {
            options.MultipartBodyLengthLimit = 10 * 1024 * 1024;
            options.ValueLengthLimit = 1024 * 1024;
            options.ValueCountLimit = 100;
        });

        services.AddDataProtection();

        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                // Разрешаем все источники (origins)
                policy.AllowAnyOrigin()
                      // Разрешаем все HTTP методы (GET, POST, PUT, DELETE, PATCH, OPTIONS и т.д.)
                      .AllowAnyMethod()
                      // Разрешаем все заголовки запросов
                      .AllowAnyHeader()
                      // Разрешаем отправку credentials (cookies, authorization headers)
                      // Примечание: AllowAnyOrigin() несовместим с AllowCredentials()
                      // Если нужны credentials, используйте SetIsOriginAllowed вместо AllowAnyOrigin
                      .SetPreflightMaxAge(TimeSpan.FromDays(1)); // Кэширование preflight запросов на 24 часа
            });
        });

        return services;
    }

    /// <summary>
    /// Добавляет сервисы приложения (Application и Infrastructure слои)
    /// </summary>
    private static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddTecDocServices(configuration);
        services.AddScoped<ITecDocArticleService, TecDocArticleService>();
        services.AddScoped<ITecDocSupplierService, TecDocSupplierService>();

        // Elasticsearch сервисы для артикулов
        services.AddSingleton<IArticleElasticsearchService, ArticleElasticsearchService>();
        services.AddHostedService<ArticleSyncBackgroundService>();

        // Elasticsearch сервисы для поставщиков
        services.AddSingleton<ISupplierElasticsearchService, SupplierElasticsearchService>();
        services.AddHostedService<SupplierSyncBackgroundService>();

        // S3 сервис для изображений
        services.AddSingleton<IS3ImageService, S3ImageService>();

        services.AddHttpClient("default")
            .AddPollyPolicies()
            .ConfigureHttpClient(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(5);
            });

        return services;
    }
}

