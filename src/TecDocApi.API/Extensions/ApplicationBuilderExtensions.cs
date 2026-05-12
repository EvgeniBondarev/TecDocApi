using TecDocApi.API.Middleware;
using TecDocApi.API.Models;
using TecDocApi.Application.Models;
using TecDocApi.Application.Services;

namespace TecDocApi.API.Extensions;

public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Настраивает HTTP request pipeline
    /// </summary>
    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
        app.UseMiddleware<SecurityHeadersMiddleware>();
        app.UseSwagger();
        
        if (app.Environment.IsDevelopment())
        {
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "TecDoc API v1");
                c.RoutePrefix = "swagger";
                c.DocumentTitle = "TecDoc API";
                c.EnableDeepLinking();
                c.DefaultModelsExpandDepth(-1);
                c.DisplayRequestDuration();
                c.EnableFilter();
                c.EnableValidator();
                c.InjectStylesheet("/swagger/custom.css");
                c.HeadContent = "<link rel=\"icon\" type=\"image/svg+xml\" href=\"/favicon.svg\">";
            });
        }

        app.UseReDoc(c =>
        {
            c.RoutePrefix = "docs";
            c.SpecUrl("/swagger/v1/swagger.json");
            c.DocumentTitle = "TecDoc API Documentation";
            c.ExpandResponses("200,201");
            c.HideHostname();
            c.HideDownloadButton();
            c.NoAutoAuth();
            c.RequiredPropsFirst();
            c.SortPropsAlphabetically();
            c.HeadContent = """
                <link rel="icon" type="image/svg+xml" href="/favicon.svg">
                <link rel="alternate icon" type="image/x-icon" href="/favicon.ico">
                <style>
                    .docs-banner {
                        position: fixed;
                        top: 0;
                        left: 0;
                        right: 0;
                        background: linear-gradient(135deg, #0c7c59, #159a71);
                        color: white;
                        padding: 12px 16px;
                        text-align: center;
                        z-index: 10000;
                        font-weight: 700;
                        box-shadow: 0 12px 28px rgba(12,124,89,0.22);
                    }
                    .docs-banner a {
                        color: #fff7cc;
                        text-decoration: underline;
                        margin: 0 8px;
                    }
                    .redoc-wrap .menu-content { padding-top: 60px; }
                    .redoc-wrap .menu-content::before {
                        content: '';
                        display: block;
                        background-image: url('http://studio-web-client.interparts.ru/logo.svg');
                        background-size: contain;
                        background-repeat: no-repeat;
                        background-position: center;
                        width: 100%;
                        height: 50px;
                        margin-bottom: 15px;
                    }
                </style>
                <script>
                    document.addEventListener('DOMContentLoaded', function() {
                        const banner = document.createElement('div');
                        banner.className = 'docs-banner';
                        banner.innerHTML = 'Новый поток картинок: сначала <strong>ArticleSearch</strong>, затем <strong>Images/article-search</strong>. Для глобального поиска используйте <strong>Images/s3-search?articleNumber=ALM2019YX</strong>. Если превью не поддерживается браузером, используйте ссылки открытия файла. <a href="/tests">Открыть Test Bench</a><a href="/swagger">Открыть Swagger UI</a>';
                        document.body.insertBefore(banner, document.body.firstChild);
                    });
                </script>
                """;
        });

        app.UseStaticFiles();
        app.UseResponseCompression();
        app.UseHttpsRedirection();
        app.UseRouting();
        
        // CORS должен быть после UseRouting и перед UseAuthorization
        // Разрешаем все источники, методы и заголовки
        app.UseCors("AllowAll");
        
        app.UseRateLimiter();
        app.UseResponseCaching();
        app.UseAuthorization();
        app.MapControllers().RequireRateLimiting("api");
        app.MapGet("/tests", async context =>
        {
            var filePath = Path.Combine(app.Environment.WebRootPath, "tests", "index.html");
            context.Response.ContentType = "text/html; charset=utf-8";
            await context.Response.SendFileAsync(filePath);
        });
        app.MapGet("/api/Images/s3-search", async (
            string articleNumber,
            int maxResults,
            IS3ImageService s3ImageService) =>
        {
            if (string.IsNullOrWhiteSpace(articleNumber))
            {
                return Results.BadRequest(new ErrorResponse
                {
                    Code = ErrorCodes.BAD_REQUEST,
                    Message = "Номер артикула не может быть пустым"
                });
            }

            var matches = await s3ImageService.SearchImagesByArticleAsync(articleNumber, null, maxResults);
            var result = matches.Select(match => new ArticleImageDocument
            {
                PictureName = match.PictureName,
                Description = match.MatchedBy,
                AdditionalDescription = match.ObjectKey,
                DocumentName = match.ObjectKey,
                DocumentType = Path.GetExtension(match.PictureName).TrimStart('.').ToUpperInvariant(),
                ShowImmediately = false,
                Url = match.Url,
                StreamUrl = match.StreamUrl,
                S3Url = match.S3Url
            }).ToList();

            return Results.Ok(result);
        })
        .WithName("SearchS3Images")
        .WithTags("Images")
        .WithSummary("Глобальный поиск изображений по articleNumber по всем папкам S3")
        .WithDescription("Ищет картинки по всем папкам S3Info без supplierId. Игнорирует регистр и спецсимволы и находит вариации вроде ALM2019YX-6, ALM2019YX_3, 1234ALM2019YX-6. Пример: /api/Images/s3-search?articleNumber=ALM2019YX&amp;maxResults=10")
        .Produces<List<ArticleImageDocument>>()
        .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
        .RequireRateLimiting("api");
        app.MapGet("/docs/index.html", context =>
        {
            context.Response.Redirect("/docs");
            return Task.CompletedTask;
        });

        // Инициализация Elasticsearch индексов при старте
        _ = Task.Run(async () =>
        {
            await Task.Delay(TimeSpan.FromSeconds(5)); // Ждем запуска Elasticsearch
            try
            {
                using var scope = app.Services.CreateScope();
                var articleElasticsearchService = scope.ServiceProvider.GetRequiredService<IArticleElasticsearchService>();
                var supplierElasticsearchService = scope.ServiceProvider.GetRequiredService<ISupplierElasticsearchService>();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                
                logger.LogInformation("Инициализация Elasticsearch индексов...");
                await articleElasticsearchService.CreateIndexAsync();
                await supplierElasticsearchService.CreateIndexAsync();
                logger.LogInformation("Инициализация Elasticsearch завершена");
            }
            catch (Exception ex)
            {
                var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger<Program>();
                logger.LogError(ex, "Ошибка при инициализации Elasticsearch");
            }
        });

        return app;
    }
}

