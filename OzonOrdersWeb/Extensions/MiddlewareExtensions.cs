using OzonOrdersWeb.Middleware;

namespace OzonOrdersWeb.Extensions;

public static class MiddlewareExtensions
{
    public static void AddCustomMiddleware(this IApplicationBuilder app)
    {
        app.UseDbInitializerMiddleware();
        app.UseErrorHandlingMiddleware();
        app.UsePreviousPageUrlMiddleware();
    }
}