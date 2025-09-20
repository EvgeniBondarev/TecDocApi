namespace OzonOrdersWeb.Middleware
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public ErrorHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            //try
            //{
                await _next(context);
            //}
            //catch (Exception ex)
            //{
            //    context.Response.ContentType = "text/html; charset=utf-8";
            //    context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            //    var errorMessage = $@"
            //                        <!DOCTYPE html>
            //                        <html lang='en'>
            //                        <head>
            //                            <meta charset='utf-8'>
            //                            <title>Ошибка</title>
            //                            <link href='https://maxcdn.bootstrapcdn.com/bootstrap/4.5.2/css/bootstrap.min.css' rel='stylesheet'>
            //                        </head>
            //                        <body>
            //                            <br/>
            //                            <div class='container'>
            //                                <div class='row'>
            //                                    <div class='col-md-6 offset-md-3'>
            //                                        <div class='alert alert-danger' role='alert'>
            //                                            <h4 class='alert-heading'>Ошибка сервера</h4>
            //                                            <p>Произошла ошибка: {ex.Message}</p>
            //                                            <hr>
            //                                            <p class='mb-0'>Попробуйте повторить запрос позже.</p>
            //                                        </div>
            //                                    </div>
            //                                </div>
            //                            </div>
            //                        </body>
            //                        </html>";

            //    await context.Response.WriteAsync(errorMessage);
            //}
        }

    }

    public static class ErrorHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseErrorHandlingMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ErrorHandlingMiddleware>();
        }
    }
}
