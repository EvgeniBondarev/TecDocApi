using Microsoft.AspNetCore.Identity;
using OzonRepositories.Context.Identity;

namespace OzonOrdersWeb.Extensions;

public static class IdentityExtensions
{
    public static void AddIdentityServices(this IServiceCollection services)
    {
        services.AddDefaultIdentity<CustomIdentityUser>()
            .AddDefaultTokenProviders()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<OzonIdentityOrderContext>();

        services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.Name = ".AspNetCore.Identity.Application";
            options.ExpireTimeSpan = TimeSpan.FromDays(1);
            options.SlidingExpiration = false;
            options.LoginPath = "/Identity/Account/Login";
            options.LogoutPath = "/Identity/Account/Logout";
            options.AccessDeniedPath = "/Identity/Account/AccessDenied";
        });
    }
}