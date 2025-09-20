using Microsoft.AspNetCore.SignalR;
using Servcies.SignalRServcies;

namespace OzonOrdersWeb.Extensions;

public static class SignalRExtensions
{
    public static void AddSignalRServices(this IServiceCollection services)
    {
        services.AddSignalR();
    }
}