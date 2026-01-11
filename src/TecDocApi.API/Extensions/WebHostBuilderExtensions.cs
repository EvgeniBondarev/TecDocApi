namespace TecDocApi.API.Extensions;

public static class WebHostBuilderExtensions
{
    /// <summary>
    /// Настраивает Kestrel для оптимизации производительности
    /// </summary>
    public static IWebHostBuilder ConfigureKestrelLimits(this IWebHostBuilder builder)
    {
        builder.ConfigureKestrel(options =>
        {
            options.Limits.MaxConcurrentConnections = 1000;
            options.Limits.MaxConcurrentUpgradedConnections = 1000;
            options.Limits.MaxRequestBodySize = 10 * 1024 * 1024; // 10 MB
            options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(2);
            options.Limits.RequestHeadersTimeout = TimeSpan.FromSeconds(30);
        });

        return builder;
    }
}

