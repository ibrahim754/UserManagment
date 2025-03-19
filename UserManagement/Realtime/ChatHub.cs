using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace UserManagement.SignalR
{
    public sealed class ChatHub : Hub
    {

        private readonly ILogger <ChatHub> _logger;
        public ChatHub(ILogger<ChatHub> logger)
        {
            _logger = logger;
        }
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
            _logger.LogInformation("SignalR is working ^_^");
            await Clients.All.SendAsync("ReceivedMessage", $"User connected with Id: {Context.ConnectionId}");
        }
        public async Task SendMessage(string user, string message)
        {
            // Broadcasts the message to all connected clients using a method "ReceiveMessage" on the client side.
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }

    }
}
