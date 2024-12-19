using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using UserManagement.Models;

namespace UserManagement.MiddleWares
{
    public class BlockedUserMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly UserManager<User> _userManager;
        public BlockedUserMiddleware(RequestDelegate next, UserManager<User> userManager)
        {
            _next = next;
            _userManager = userManager;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                // Get the currently authenticated user
                //await _userManager.setlo
                var user = await _userManager.GetUserAsync(context.User);
                // Check if user is blocked
                if (user != null && user.IsBlocked)
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    await context.Response.WriteAsync("User is blocked due to policy violations.");
                    return; // Stop further request processing
                }
            }

            await _next(context); // Continue to the next middleware or endpoint
        }
    }
}
