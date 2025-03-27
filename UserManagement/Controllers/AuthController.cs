using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UserManagement.DTOs;
using UserManagement.Interfaces;

namespace UserManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(IAuthService authService, ILogger<RegistrationController> logger)
        : BaseController
    {
        [HttpPost("logIn")]
        public async Task<IActionResult> GetTokenAsync(TokenRequestModel model)
        {

            logger.LogInformation("Attempting to generate token for user with email {Email}", model.Email);

            var userAgent = new UserAgent
            {
                UserDevice = HttpContext.Request.Headers["User-Agent"].ToString(),
                UserIp = HttpContext.Connection.RemoteIpAddress?.ToString()
            };

            var result = await authService.LogInAsync(model, userAgent);

            return result.Match(
                authModel =>
                {
                    if (!string.IsNullOrEmpty(authModel.RefreshToken))
                        SetRefreshTokenInCookie(authModel.RefreshToken, authModel.RefreshTokenExpiration);

                    logger.LogInformation("Token generated successfully for user {UserName}", authModel.Username);
                    return Ok(authModel);
                },
                errors =>
                {
                    logger.LogWarning("Token generation failed for user with email {Email}", model.Email);
                    return Problem(errors);
                });

        }
        private void SetRefreshTokenInCookie(string refreshToken, DateTime expires)
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
                logger.LogInformation("Refresh token set in cookie, expires at {Expiration}", expires);

        }

    }
}
