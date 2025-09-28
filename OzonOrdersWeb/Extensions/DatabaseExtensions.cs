using Microsoft.EntityFrameworkCore;
using OzonRepositories.Context;
using OzonRepositories.Context.Identity;
using OzonRepositories.Utils;

namespace OzonOrdersWeb.Extensions;

public static class DatabaseExtensions
{
    public static void AddDatabaseServices(this IServiceCollection services, IConfiguration configuration, bool isProd)
    {
        string sqlConnection = isProd 
            ? configuration.GetConnectionString("MySqlConnectionProd") 
            : configuration.GetConnectionString("MySqlConnectionLocal");
        string jcEtalonConnection = configuration.GetConnectionString("JcEtalonConnectionProd");
        
        string bitrixConnection =  isProd 
            ? configuration.GetConnectionString("BitrixConnectionProd")
            : configuration.GetConnectionString("BitrixConnectionLocal");

        services.AddDbContext<OzonOrderContext>(options =>
            options.UseMySql(sqlConnection, ServerVersion.AutoDetect(sqlConnection)));

        services.AddDbContext<OzonIdentityOrderContext>(options =>
            options.UseMySql(sqlConnection, ServerVersion.AutoDetect(sqlConnection)));
        
        services.AddDbContext<JcEtalonContext>(options =>
            options.UseSqlServer(jcEtalonConnection));
        
        services.AddDbContext<BitrixContext>(options =>
            options.UseMySql(bitrixConnection, ServerVersion.AutoDetect(bitrixConnection)));

        services.AddTransient<DbInitializer>();
    }
}