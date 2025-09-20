using Hangfire.Annotations;
using Hangfire.Dashboard;

namespace OzonOrdersWeb.Services.HangfireAuthorization
{
    public class HangfireAuthorization : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();

            return httpContext.User.IsInRole("Admin");
        }
    }
}
