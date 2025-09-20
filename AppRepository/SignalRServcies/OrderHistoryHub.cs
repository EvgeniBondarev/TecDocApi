using Microsoft.AspNetCore.SignalR;
using Servcies.DataServcies;
using System.Threading.Tasks;

namespace Servcies.SignalRServcies
{
    public class OrderHistoryHub : Hub
    {
        private readonly IOrderHistoryDataService _orderHistoryService;

        public OrderHistoryHub(IOrderHistoryDataService orderHistoryService)
        {
            _orderHistoryService = orderHistoryService;
        }

        // Этот метод будет вызываться из JavaScript
        public async Task RequestRecentChanges()
        {
            try
            {
                var changes = await _orderHistoryService.GetRecentChangesAsync(50);
                
                // Преобразуем для безопасной передачи
                var safeChanges = changes.Select(c => new
                {
                    c.Id,
                    c.OrderId,
                    ColumnName = c.ColumnName ?? "",
                    ColumnDisplayName = c.ColumnDisplayName ?? "",
                    OldValue = c.OldValue,
                    NewValue = c.NewValue,
                    ChangedAt = c.ChangedAt,
                    ChangedBy = c.ChangedBy ?? ""
                }).ToList();
                
                // Отправляем данные обратно caller'у
                await Clients.Caller.SendAsync("ReceiveRecentChanges", safeChanges);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in RequestRecentChanges: {ex.Message}");
                // Можно отправить ошибку клиенту
                await Clients.Caller.SendAsync("ReceiveError", ex.Message);
            }
        }

        // Метод для отправки уведомлений о новых изменениях
        public async Task SendChangeNotification(object change)
        {
            await Clients.All.SendAsync("ReceiveChange", change);
        }
    }
}