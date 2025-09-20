using Microsoft.AspNetCore.SignalR;

namespace Servcies.SignalRServcies;

public static class NotificationService
{
    private static IHubContext<NotificationHub>? _hubContext;
    
    public static void Initialize(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }
    
    public static async Task NotifyAllAsync(string message)
    {
        if (_hubContext == null)
            throw new InvalidOperationException("NotificationService не инициализирован.");

        await _hubContext.Clients.All.SendAsync("PopupNotification", message);
    }
}