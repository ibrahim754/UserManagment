using Microsoft.AspNetCore.SignalR;

namespace UserManagement.Realtime
{
    public abstract class RealTimeHubBase : Hub
    {
        public  override async Task OnConnectedAsync()=> await base.OnConnectedAsync();
        public  override async Task OnDisconnectedAsync(Exception? exception) => await base.OnDisconnectedAsync(exception);

        public abstract Task CustomMethod1();
        public abstract Task CustomMethod2();
    }
}
