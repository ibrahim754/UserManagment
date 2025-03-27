using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UserManagement.DTOs;
using UserManagement.Interfaces;

namespace UserManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TokenController(ITokenService tokenService, ILogger<TokenController> logger)
        : BaseController
    {
        [HttpPost("revokeToken")]
        public async Task<IActionResult> RevokeToken([FromBody] RevokeToken model)
        {

            logger.LogInformation("Attempting to revoke token.");

            var token = model.Token ?? Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(token))
            {
                logger.LogWarning("Token revocation failed: Token is required.");
                return BadRequest("Token is required!");
            }

            var result = await tokenService.RevokeTokenAsync(token);

            return result.Match(
                Success =>
                {
                    logger.LogInformation("Token revoked successfully.");
                    return Ok();
                },
                errors =>
                {
                    logger.LogWarning("Token revocation failed.");
                    return Problem(errors);
                });

        }

        [HttpGet("refreshToken")]
        public async Task<IActionResult> RefreshToken()
        {

            logger.LogInformation("Attempting to refresh token.");

            var tokenRequest = new RefreshTokenRequest
            {
                RefreshToken = Request.Cookies["refreshToken"],
                UserDeviceId = HttpContext.Request.Headers["User-Agent"].ToString(),
                UserIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
            };

            var result = await tokenService.RefreshTokenAsync(tokenRequest);

            return result.Match(
                authModel =>
                {
                    SetRefreshTokenInCookie(authModel.RefreshToken, authModel.RefreshTokenExpiration);
                    logger.LogInformation("Token refreshed successfully for user {UserName}.", authModel.Username);
                    return Ok(authModel);
                },
                errors =>
                {
                    logger.LogWarning("Token refresh failed.");
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
            logger.LogInformation("Refresh token set in cookie, expires at {Expiration}.", expires);

        }
    }
}
