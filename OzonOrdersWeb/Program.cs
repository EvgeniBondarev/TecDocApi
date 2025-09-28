using System.Globalization;
using Hangfire;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.SignalR;
using OzonOrdersWeb.Extensions;
using OzonOrdersWeb.Services.HangfireAuthorization;
using Servcies.DataServcies;
using Servcies.ReleasServcies.ReleaseManager;
using Servcies.SignalRServcies;
using Serilog;
using Prometheus;

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

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()           
    .WriteTo.Seq("http://seq:5341") 
    .CreateLogger();

builder.Services.AddResponseCaching();
builder.Host.UseSerilog();

builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true; 
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});

builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = System.IO.Compression.CompressionLevel.Fastest;
});

builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = System.IO.Compression.CompressionLevel.Optimal;
});

var app = builder.Build();

app.UseResponseCaching(); 
app.UseResponseCompression();
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=604800");
    }
});

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.All
});
app.UseDeveloperExceptionPage();

app.UseSession();

app.AddCustomMiddleware();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseMetricServer(); 
app.UseHttpMetrics();           

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
    pattern: "{controller=Orders}/{action=Index}/{id?}",
    defaults: new { area = "Studio2" });

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

//app.UseExceptionHandler("/Home/Error");

app.Run();
