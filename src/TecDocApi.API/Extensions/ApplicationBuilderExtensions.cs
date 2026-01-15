using TecDocApi.API.Middleware;

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
                        banner.style.cssText = 'position: fixed; top: 0; left: 0; right: 0; background: #4CAF50; color: white; padding: 10px; text-align: center; z-index: 10000; font-weight: bold;';
                        banner.innerHTML = 'Для выполнения запросов используйте <a href="/swagger" style="color: #FFD700; text-decoration: underline;">Swagger UI</a>';
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

        // Инициализация Elasticsearch индексов при старте
        _ = Task.Run(async () =>
        {
            await Task.Delay(TimeSpan.FromSeconds(5)); // Ждем запуска Elasticsearch
            try
            {
                using var scope = app.Services.CreateScope();
                var articleElasticsearchService = scope.ServiceProvider.GetRequiredService<TecDocApi.Application.Services.IArticleElasticsearchService>();
                var supplierElasticsearchService = scope.ServiceProvider.GetRequiredService<TecDocApi.Application.Services.ISupplierElasticsearchService>();
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

