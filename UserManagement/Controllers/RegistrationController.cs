using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Framework;
using Microsoft.Extensions.Logging;
using UserManagement.DTOs;
using UserManagement.Interfaces;

namespace UserManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RegistrationController(IRegistrationService authService, ILogger<RegistrationController> logger)
        : BaseController
    {
        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync(RegisterModel model)
        {

            logger.LogInformation("Attempting to register user with email {Email}", model.Email);

            var userAgent = new UserAgent
            {
                UserDevice = HttpContext.Request.Headers["User-Agent"].ToString(),
                UserIp = HttpContext.Connection?.RemoteIpAddress?.ToString()
            };

            var result = await authService.RegisterAsync(model, userAgent, null);

            return result.Match(
                Guid =>
                {

                    return Ok(Guid);
                },
                errors =>
                {
                    logger.LogWarning("User registration failed for email {Email}", model.Email);
                    return Problem(errors);
                });

        }

        [HttpPost("confirm")]
        public async Task<IActionResult> ConfirmAsync(ConfirmationUserDto confirmationUser)
        {
            logger.LogInformation("Attempting to register user with register id {id}", confirmationUser.registerationId);

            var userAgent = new UserAgent
            {
                UserDevice = HttpContext.Request.Headers["User-Agent"].ToString(),
                UserIp = HttpContext.Connection.RemoteIpAddress?.ToString()
            };

            var result = await authService.ConfirmRegisterAsync(confirmationUser, userAgent);

            return result.Match(
                authModel =>
                {
                    SetRefreshTokenInCookie(authModel.RefreshToken, authModel.RefreshTokenExpiration);
                    logger.LogInformation("User {UserName} registered successfully", authModel.Username);
                    return Ok(authModel);
                },
                errors =>
                {
                    logger.LogWarning("User registration failed for register id {id}", confirmationUser.registerationId);
                    return Problem(errors);
                });

        }
        private void SetRefreshTokenInCookie(string? refreshToken, DateTime expires)
        {

            if (refreshToken is null)
            {
                return;
            }
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = expires.ToLocalTime(),
                Secure = true,
                IsEssential = true,
                SameSite = SameSiteMode.None
            };

            Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
            logger.LogInformation("Refresh token set in cookie, expires at {Expiration}", expires);

        }
    }
}
