using System.Globalization;
using Hangfire;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.SignalR;
using OzonOrdersWeb.Extensions;
using OzonOrdersWeb.Services.HangfireAuthorization;
using Servcies.DataServcies;
using Servcies.ReleasServcies.ReleaseManager;
using Servcies.SignalRServcies;

var builder = WebApplication.CreateBuilder(args);

#if DEBUG
bool isProd = false;
#else
bool isProd = true;
#endif

string regionCode = isProd ? "en" : "ru";

builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient(); 

builder.Services.AddDatabaseServices(builder.Configuration, isProd);
builder.Services.AddHangfireServices(builder.Configuration, isProd);
builder.Services.AddIdentityServices();
builder.Services.AddRepositories(builder.Configuration);
builder.Services.AddCacheServices();
builder.Services.AddApiClients(builder.Configuration);
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddSignalRServices();

builder.Services.AddSingleton<ReleaseManager>(provider =>
{
    var releaseManager = ReleaseManager.Instance;
    releaseManager.SetRegionCode(regionCode);
    return releaseManager;
});

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = 52428800; 
});

var app = builder.Build();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.All
});

app.UseSession();

app.AddCustomMiddleware();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseHangfireDashboard("/Hangfire", new DashboardOptions
{
    Authorization = [new HangfireAuthorization()]
});

app.MapHub<NotificationHub>("/notificationHub");
app.MapHub<OrderHistoryHub>("/orderHistoryHub");

var hubContext = app.Services.GetRequiredService<IHubContext<NotificationHub>>();
NotificationService.Initialize(hubContext);

app.MapRazorPages();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Orders}/{action=IndexV2}/{id?}",
    defaults: new { area = "Studio2" });

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.Run();
