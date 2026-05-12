using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TecDocApi.Infrastructure.Data;
using TecDocApi.Infrastructure.Repositories;

namespace TecDocApi.Infrastructure.Extensions;

public static class TecDocServiceExtensions
{
    public static IServiceCollection AddTecDocServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Приоритет: переменная окружения > appsettings.json
        // ASP.NET Core автоматически читает переменные окружения через IConfiguration
        // Формат: ConnectionStrings__TecDocDatabase или TECDOC_DATABASE_CONNECTION_STRING
        var envConnectionString = Environment.GetEnvironmentVariable("TECDOC_DATABASE_CONNECTION_STRING");
        var configConnectionString = configuration.GetConnectionString("TecDocDatabase");
        
        // Убираем кавычки, если они есть (Docker Compose может добавлять кавычки для значений со спецсимволами)
        if (!string.IsNullOrWhiteSpace(envConnectionString))
        {
            envConnectionString = envConnectionString.Trim().Trim('\'').Trim('"');
            
            // Если пароль URL-кодирован, декодируем его
            // Проверяем наличие URL-кодированных символов (%XX)
            if (envConnectionString.Contains("%"))
            {
                try
                {
                    envConnectionString = Uri.UnescapeDataString(envConnectionString);
                    Console.WriteLine("[TecDocServiceExtensions] Выполнено URL-декодирование строки подключения");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[TecDocServiceExtensions] Ошибка при URL-декодировании: {ex.Message}");
                }
            }
        }
        
        var tecDocConnectionString = envConnectionString ?? configConnectionString;
        
        if (string.IsNullOrWhiteSpace(tecDocConnectionString))
        {
            throw new InvalidOperationException(
                "Строка подключения TecDocDatabase не найдена. " +
                "Установите переменную окружения TECDOC_DATABASE_CONNECTION_STRING или " +
                "настройте ConnectionStrings:TecDocDatabase в appsettings.json");
        }
        
        // Логируем источник (без самой строки для безопасности)
        var source = envConnectionString != null 
            ? "переменная окружения TECDOC_DATABASE_CONNECTION_STRING" 
            : "appsettings.json";
        
        // Используем Console для логирования, так как ILogger еще не доступен
        Console.WriteLine($"[TecDocServiceExtensions] Строка подключения загружена из: {source}");
        
        // Отладочное логирование - проверяем наличие пароля в строке
        var passwordStartIndex = tecDocConnectionString.IndexOf("Password=", StringComparison.OrdinalIgnoreCase);
        if (passwordStartIndex >= 0)
        {
            var passwordPart = tecDocConnectionString.Substring(passwordStartIndex);
            var passwordEndIndex = passwordPart.IndexOf(';');
            if (passwordEndIndex > 0)
            {
                var passwordValue = passwordPart.Substring(9, passwordEndIndex - 9); // "Password=" = 9 символов
                Console.WriteLine($"[TecDocServiceExtensions] Пароль найден, длина: {passwordValue.Length} символов");
                // Показываем первые и последние символы пароля для отладки (безопасно)
                if (passwordValue.Length > 4)
                {
                    var passwordPreview = passwordValue.Substring(0, 2) + "..." + passwordValue.Substring(passwordValue.Length - 2);
                    Console.WriteLine($"[TecDocServiceExtensions] Предпросмотр пароля: {passwordPreview}");
                }
            }
        }
        else
        {
            Console.WriteLine("[TecDocServiceExtensions] ВНИМАНИЕ: Пароль не найден в строке подключения!");
        }
        
        // Отладочное логирование (первые 80 символов для проверки)
        var preview = tecDocConnectionString.Length > 80 
            ? tecDocConnectionString.Substring(0, 80) + "..." 
            : tecDocConnectionString;
        Console.WriteLine($"[TecDocServiceExtensions] Предпросмотр строки подключения: {preview}");

        // TecDoc MySQL база данных - используем DbContextFactory для параллелизма
        // И DbContextPool для оптимизации производительности (+10-25%)
        services.AddDbContextPool<TecDocContext>(options =>
        {
            if (!string.IsNullOrWhiteSpace(tecDocConnectionString))
            {
                // Используем ServerVersion для MySQL 8.0
                var serverVersion = ServerVersion.Parse("8.0.0-mysql");
                options.UseMySql(tecDocConnectionString, serverVersion, mySqlOptions =>
                {
                    mySqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3, // Уменьшено для быстрого отказа при проблемах
                        maxRetryDelay: TimeSpan.FromSeconds(2),
                        errorNumbersToAdd: null
                    );
                    mySqlOptions.CommandTimeout(10); // Таймаут 10 секунд для предотвращения бесконечных ожиданий
                });
            }
            options.EnableSensitiveDataLogging(false);
            options.EnableServiceProviderCaching();
            // Отключаем отслеживание изменений для режима только чтения
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        }, poolSize: 128); // Размер пула для оптимизации

        // Также регистрируем Factory для параллельных запросов
        services.AddDbContextFactory<TecDocContext>(options =>
        {
            if (!string.IsNullOrWhiteSpace(tecDocConnectionString))
            {
                var serverVersion = ServerVersion.Parse("8.0.0-mysql");
                options.UseMySql(tecDocConnectionString, serverVersion, mySqlOptions =>
                {
                    mySqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3, // Уменьшено для быстрого отказа при проблемах
                        maxRetryDelay: TimeSpan.FromSeconds(2),
                        errorNumbersToAdd: null
                    );
                    mySqlOptions.CommandTimeout(10); // Таймаут 10 секунд для предотвращения бесконечных ожиданий
                });
            }
            options.EnableSensitiveDataLogging(false);
            options.EnableServiceProviderCaching();
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        });

        // Регистрация сервисов с фабрикой
        services.AddScoped<TecDocUnitOfWork>();

        return services;
    }
}

