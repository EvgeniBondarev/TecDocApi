using Microsoft.AspNetCore.Identity;
using OzonRepositories.Context.Identity;
using OzonRepositories.Utils;

namespace OzonOrdersWeb.Middleware
{
    public class DbInitializerMiddleware
    {
        private readonly RequestDelegate _next;

        public DbInitializerMiddleware(RequestDelegate next)
        {
            _next = next;

        }

        public Task Invoke(HttpContext httpContext, OzonIdentityOrderContext db,
                           RoleManager<IdentityRole> roleManager, UserManager<CustomIdentityUser> userManager)
        {
            if (!(httpContext.Session.Keys.Contains("starting")))
            {
                DbInitializer dbInitializer = new DbInitializer(db, roleManager, userManager);
                dbInitializer.InitializeDb();

                httpContext.Session.SetString("starting", "Yes");
            }
            return _next.Invoke(httpContext);

        }
    }
    public static class DbInitializerMiddlewareExtensions
    {
        public static IApplicationBuilder UseDbInitializerMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<DbInitializerMiddleware>();
        }
    }
}
