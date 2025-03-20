using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using UserManagement.Realtime;

namespace UserManagement.SignalR
{
    public sealed class ChatHub : RealTimeHubBase
    {
        private readonly ILogger<ChatHub> _logger;

        // Thread-safe collection mapping connection IDs to user names.
        private static readonly ConcurrentDictionary<string, string> OnlineUsers =
            new ConcurrentDictionary<string, string>();
        public ChatHub(ILogger<ChatHub> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            // Optionally call the Hub's default implementation.
            await base.OnConnectedAsync();

            var userName = Context.User?.Identity?.Name ?? Context.ConnectionId;
            OnlineUsers.TryAdd(Context.ConnectionId, userName);

            await Clients.All.SendAsync("ReceivedMessage", $"User connected with Id: {Context.ConnectionId}");
            _logger.LogInformation("User with id {id} joined live", Context.ConnectionId);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
            _logger.LogInformation("User with id {id} disconnected", Context.ConnectionId);
            await Clients.Others.SendAsync("OnDisconnectedAsync", $"User with id {Context.ConnectionId} disconnected");
        }
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
        public override Task CustomMethod1()
        {
            // Implementation for custom method 1.
            return Task.CompletedTask;
        }

        public override Task CustomMethod2()
        {
            // Implementation for custom method 2.
            return Task.CompletedTask;
        }

       
    }
}
