using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Http.Features;
using SlqStudio.Application.Services.AppSettingsServices;
using SlqStudio.Application.Services.EmailService;
using SlqStudio.Application.Services.EmailService.Implementation;
using SlqStudio.Application.Services.EmailService.Models;

namespace OzonOrdersWeb.Extensions;

public static class ApplicationServicesExtensions
{
    public static void AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<IISServerOptions>(options =>
        {
            options.MaxRequestBodySize = int.MaxValue;
        });

        services.Configure<KestrelServerOptions>(options =>
        {
            options.Limits.MaxRequestBodySize = 314572800;
        });

        services.Configure<FormOptions>(options =>
        {
            options.ValueCountLimit = 4096 * 4;
        });

        services.Configure<SmtpSettings>(configuration.GetSection("SmtpSettings"));
        services.AddTransient<IEmailService, EmailService>();

        services.AddSingleton<IAppSettingsService, AppSettingsService>();
        services.AddSingleton<AppSettingsBuilder>();
    }
}