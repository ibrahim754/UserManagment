using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UserManagement.DTOs;
using UserManagement.Interfaces;

namespace UserManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : BaseController
    {
        private readonly IAuthService _authService;
        private readonly ILogger<RegistrationController> _logger;
        public AuthController(IAuthService authService, ILogger<RegistrationController> logger)
        {
            _authService = authService;
            _logger = logger;
        }
        [HttpPost("logIn")]
        public async Task<IActionResult> GetTokenAsync( [FromQuery]TokenRequestModel model)
        {
            try
            {
                _logger.LogInformation("Attempting to generate token for user with email {Email}", model.Email);

                var userAgent = new UserAgent
                {
                    UserDevice = HttpContext.Request.Headers["User-Agent"].ToString(),
                    UserIp = HttpContext.Connection.RemoteIpAddress?.ToString()
                };

                var result = await _authService.LogInAsync(model, userAgent);

                return result.Match(
                    authModel =>
                    {
                        if (!string.IsNullOrEmpty(authModel.RefreshToken))
                            SetRefreshTokenInCookie(authModel.RefreshToken, authModel.RefreshTokenExpiration);

                        _logger.LogInformation("Token generated successfully for user {UserName}", authModel.Username);
                        return Ok(authModel);
                    },
                    errors =>
                    {
                        _logger.LogWarning("Token generation failed for user with email {Email}", model.Email);
                        return Problem(errors);
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while generating token for user with email {Email}", model.Email);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred during token generation.");
            }
        }
        private void SetRefreshTokenInCookie(string refreshToken, DateTime expires)
        {
            try
            {
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to set refresh token in cookie.");
            }
        }
    }
}
