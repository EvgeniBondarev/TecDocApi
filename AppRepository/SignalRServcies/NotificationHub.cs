using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace Servcies.SignalRServcies
{
    public class NotificationHub : Hub
    {
        private static readonly ConcurrentDictionary<string, string> ConnectedUsers = new();

        public override async Task OnConnectedAsync()
        {
            var connectionId = Context.ConnectionId;
            ConnectedUsers.TryAdd(connectionId, Context.User?.Identity?.Name ?? "Anonymous");
            await base.OnConnectedAsync();
            await Clients.All.SendAsync("UserConnected", ConnectedUsers.Count);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var connectionId = Context.ConnectionId;
            ConnectedUsers.TryRemove(connectionId, out _);
            await base.OnDisconnectedAsync(exception);
            await Clients.All.SendAsync("UserDisconnected", ConnectedUsers.Count);
        }

        public Task<int> GetConnectedUsersCount()
        {
            return Task.FromResult(ConnectedUsers.Count);
        }

        public Task<IEnumerable<string>> GetConnectedUsers()
        {
            return Task.FromResult(ConnectedUsers.Values.AsEnumerable());
        }
    }
}