using Microsoft.AspNetCore.Builder;
using UserManagement.SignalR;

namespace UserManagement.Extensions
{
    public static class UserManagmentAppConfig
    {
        public static WebApplication ConfigureApplication(this WebApplication app)
        {
            app.UseWebSockets();
            app.MapHub<ChatHub>("/Chat-Hub");
            return app;
        }
    }
}
