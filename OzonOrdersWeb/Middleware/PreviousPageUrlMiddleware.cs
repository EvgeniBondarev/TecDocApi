using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace OzonOrdersWeb.Middleware
{


    public class PreviousPageUrlMiddleware
    {
        private readonly RequestDelegate _next;

        public PreviousPageUrlMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            // Получаем URL предыдущей страницы из заголовка Referer
            var refererUrl = context.Request.Headers["Referer"].ToString();

            // Обновляем значение в куки
            context.Response.Cookies.Append("PreviousPageUrl", refererUrl);

            await _next(context);
        }
    }
    public static class PreviousPageUrlMiddlewareExtensions
    {
        public static IApplicationBuilder UsePreviousPageUrlMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ErrorHandlingMiddleware>();
        }
    }

}
