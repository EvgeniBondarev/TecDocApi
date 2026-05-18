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
        var tecDocConnectionString = ResolveTecDocConnectionString(configuration);

        // TecDoc MySQL база данных - DbContextPool для оптимизации производительности (+10-25%)
        services.AddDbContextPool<TecDocContext>(options =>
        {
            ConfigureMySqlOptions(options, tecDocConnectionString);
        }, poolSize: 128);

        // Factory для параллельных запросов
        services.AddDbContextFactory<TecDocContext>(options =>
        {
            ConfigureMySqlOptions(options, tecDocConnectionString);
        });

        services.AddScoped<TecDocUnitOfWork>();

        return services;
    }

    /// <summary>
    /// Определяет строку подключения TecDoc с учётом приоритета:
    /// переменная окружения TECDOC_DATABASE_CONNECTION_STRING > appsettings.json.
    /// Вызывается также из API слоя для заполнения DatabaseConnectionOptions.
    /// </summary>
    private static string ResolveTecDocConnectionString(IConfiguration configuration)
    {
        var envConnectionString = Environment.GetEnvironmentVariable("TECDOC_DATABASE_CONNECTION_STRING");
        var configConnectionString = configuration.GetConnectionString("TecDocDatabase");

        // Убираем кавычки, которые Docker Compose может добавлять для значений со спецсимволами
        if (!string.IsNullOrWhiteSpace(envConnectionString))
        {
            envConnectionString = envConnectionString.Trim().Trim('\'').Trim('"');

            // Декодируем URL-кодированные символы (%XX), если они есть
            if (envConnectionString.Contains('%'))
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

        var resolved = envConnectionString ?? configConnectionString;

        if (string.IsNullOrWhiteSpace(resolved))
        {
            throw new InvalidOperationException(
                "Строка подключения TecDocDatabase не найдена. " +
                "Установите переменную окружения TECDOC_DATABASE_CONNECTION_STRING или " +
                "настройте ConnectionStrings:TecDocDatabase в appsettings.json");
        }

        var source = envConnectionString != null
            ? "переменная окружения TECDOC_DATABASE_CONNECTION_STRING"
            : "appsettings.json";
        Console.WriteLine($"[TecDocServiceExtensions] Строка подключения загружена из: {source}");

        LogConnectionStringDebugInfo(resolved);

        return resolved;
    }

    private static void ConfigureMySqlOptions(DbContextOptionsBuilder options, string connectionString)
    {
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            var serverVersion = ServerVersion.Parse("8.0.0-mysql");
            options.UseMySql(connectionString, serverVersion, mySqlOptions =>
            {
                mySqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(2),
                    errorNumbersToAdd: null);
                mySqlOptions.CommandTimeout(10);
            });
        }

        options.EnableSensitiveDataLogging(false);
        options.EnableServiceProviderCaching();
        // Отключаем отслеживание изменений — режим только чтения
        options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
    }

    private static void LogConnectionStringDebugInfo(string connectionString)
    {
        var passwordStartIndex = connectionString.IndexOf("Password=", StringComparison.OrdinalIgnoreCase);
        if (passwordStartIndex >= 0)
        {
            var passwordPart = connectionString[passwordStartIndex..];
            var passwordEndIndex = passwordPart.IndexOf(';');
            if (passwordEndIndex > 0)
            {
                var passwordValue = passwordPart[9..passwordEndIndex]; // "Password=" = 9 символов
                Console.WriteLine($"[TecDocServiceExtensions] Пароль найден, длина: {passwordValue.Length} символов");
                if (passwordValue.Length > 4)
                {
                    var preview = passwordValue[..2] + "..." + passwordValue[^2..];
                    Console.WriteLine($"[TecDocServiceExtensions] Предпросмотр пароля: {preview}");
                }
            }
        }
        else
        {
            Console.WriteLine("[TecDocServiceExtensions] ВНИМАНИЕ: Пароль не найден в строке подключения!");
        }

        var connPreview = connectionString.Length > 80
            ? connectionString[..80] + "..."
            : connectionString;
        Console.WriteLine($"[TecDocServiceExtensions] Предпросмотр строки подключения: {connPreview}");
    }
}
