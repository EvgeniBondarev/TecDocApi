using Hangfire;
using Hangfire.Dashboard;
using Hangfire.MySql;
using OzonOrdersWeb.Services.HangfireAuthorization;

namespace OzonOrdersWeb.Extensions;

public static class HangfireExtensions
{
    public static void AddHangfireServices(this IServiceCollection services, IConfiguration configuration, bool isProd)
    {
        string hangfireConnection = isProd
            ? configuration.GetConnectionString("HangfireCnonectionProd")
            : configuration.GetConnectionString("HangfireConnectionLocal");

        services.AddHangfire(configuration =>
        {
            configuration.UseStorage(
                new MySqlStorage(
                    hangfireConnection,
                    new MySqlStorageOptions
                    {
                        TablesPrefix = "Hangfire"
                    }
                )
            );
        });

        services.AddHangfireServer(options =>
        {
            options.Queues = ["upload-queue-new"];
            options.WorkerCount = 1;
        });
    }
}