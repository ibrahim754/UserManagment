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
    public class RegistrationController : BaseController
    {
        private readonly IRegistrationService _authService;
        private readonly ILogger<RegistrationController> _logger;
        public RegistrationController(IRegistrationService authService, ILogger<RegistrationController> logger)
        {
            _authService = authService;
            _logger = logger;
        }
        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync(RegisterModel model)
        {

            _logger.LogInformation("Attempting to register user with email {Email}", model.Email);

            var userAgent = new UserAgent
            {
                UserDevice = HttpContext.Request.Headers["User-Agent"].ToString(),
                UserIp = HttpContext.Connection?.RemoteIpAddress?.ToString()
            };

            var result = await _authService.RegisterAsync(model, userAgent, null);

            return result.Match(
                Guid =>
                {

                    return Ok(Guid);
                },
                errors =>
                {
                    _logger.LogWarning("User registration failed for email {Email}", model.Email);
                    return Problem(errors);
                });

        }

        [HttpPost("confirm")]
        public async Task<IActionResult> ConfirmAsync(ConfirmationUserDto confirmationUser)
        {
            _logger.LogInformation("Attempting to register user with register id {id}", confirmationUser.registerationId);

            var userAgent = new UserAgent
            {
                UserDevice = HttpContext.Request.Headers["User-Agent"].ToString(),
                UserIp = HttpContext.Connection.RemoteIpAddress?.ToString()
            };

            var result = await _authService.ConfirmRegisterAsync(confirmationUser, userAgent);

            return result.Match(
                authModel =>
                {
                    SetRefreshTokenInCookie(authModel.RefreshToken, authModel.RefreshTokenExpiration);
                    _logger.LogInformation("User {UserName} registered successfully", authModel.Username);
                    return Ok(authModel);
                },
                errors =>
                {
                    _logger.LogWarning("User registration failed for register id {id}", confirmationUser.registerationId);
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
            _logger.LogInformation("Refresh token set in cookie, expires at {Expiration}", expires);

        }
    }
}
