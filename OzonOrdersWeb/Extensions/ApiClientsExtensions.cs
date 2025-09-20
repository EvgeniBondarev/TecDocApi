using Microsoft.Extensions.Caching.Memory;
using PartsInfo.HttpUtils;
using Servcies.ApiServcies.AbcpApi;
using Servcies.ApiServcies.AvdApiConfig;
using Servcies.ApiServcies.DropBoxApi;
using Servcies.ApiServcies.InterpartsApi;
using Servcies.ApiServcies.OzonApi;
using Servcies.ApiServcies.TecDocApi;
using Servcies.ApiServcies.TradesoftApi;
using Servcies.ApiServcies.TradesoftApi.Models;
using Servcies.ApiServcies.YandexApi;
using Servcies.ApiServcies.ZzapApi;

namespace OzonOrdersWeb.Extensions;

public static class ApiClientsExtensions
{
    public static void AddApiClients(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<OzonJsonDataBuilder>(serviceProvider =>
        {
            var clientId = configuration.GetConnectionString("ClientId");
            var apiKey = configuration.GetConnectionString("ApiKey");

            var jsonDataBuilder = new OzonJsonDataBuilder();
            if (clientId == null) return jsonDataBuilder;
            if (apiKey != null)
                jsonDataBuilder.Init(clientId, apiKey);
            return jsonDataBuilder;
        });

        services.AddTransient<YandexDataManager>(serviceProvider =>
        {
            var apiKey = configuration.GetConnectionString("YandexApiKey");
            var yandexDataManager = new YandexDataManager();
            if (apiKey != null) yandexDataManager.SetClient(apiKey);
            return yandexDataManager;
        });

        services.AddTransient<InterpartsApiDataManager>(serviceProvider =>
        {
            var apiKey = configuration.GetConnectionString("InterpartsApiKey");
            var interpartsApiDataManager = new InterpartsApiDataManager();
            if (apiKey != null) interpartsApiDataManager.SetClient(apiKey);
            return interpartsApiDataManager;
        });

        services.AddSingleton(new DropboxApiClient(
            configuration.GetConnectionString("DropBoxRefreshToken"),
            configuration.GetConnectionString("DropBoxAppKey"),
            configuration.GetConnectionString("DropBoxAppSecret")));

        services.AddTransient<TecDocApiClient>();
        services.AddTransient<TecDocDataManager>();

        services.AddSingleton(new TradesoftApiConfig
        {
            User = configuration["Tradesoft:User"],
            Password = configuration["Tradesoft:Password"],
        });

        services.AddSingleton(new MaxiPartsConfig
        {
            User = configuration["MaxiParts:User"],
            Password = configuration["MaxiParts:Password"],
        });

        services.AddTransient<TradesoftApiClient>();
        services.AddTransient<TradesoftDataManager>();

        services.AddSingleton(new ZzapApiConfig
        {
            ApiKey = configuration["Zzap:ApiKey"],
            Login = configuration["Zzap:Login"],
            Password = configuration["Zzap:Password"]
        });
        services.AddTransient<ZzapDataManager>(serviceProvider =>
        {
            var config = serviceProvider.GetRequiredService<ZzapApiConfig>();
            var cache = serviceProvider.GetRequiredService<IMemoryCache>();
            return new ZzapDataManager(config, cache);
        });

        services.AddSingleton(new AbcpApiConfig
        {
            UserLogin = configuration["Abcp:UserLogin"],
            UserPsw = configuration["Abcp:UserPsw"],
            Domain = configuration["Abcp:Domain"]
        });
        
        services.AddSingleton<AbcpDataManager>(serviceProvider =>
        {
            var config = serviceProvider.GetRequiredService<AbcpApiConfig>();
            var cache = serviceProvider.GetRequiredService<IMemoryCache>();
            return new AbcpDataManager(config, cache);
        });
        services.AddTransient<AbcpApiClient>();
        
        services.AddSingleton(new AvdApiConfig
        {
            Login = configuration["Avd:Login"],
            
            Password = configuration["Avd:Password"],
        });
        
        services.AddTransient<AvdDataManager>(serviceProvider =>
        {
            var config = serviceProvider.GetRequiredService<AvdApiConfig>();
            var cache = serviceProvider.GetRequiredService<IMemoryCache>();
            return new AvdDataManager(config, cache);
        });
        services.AddTransient<AvdApiClient>();
        
        services.AddScoped<ProxyHttpClientService>();
    }
}