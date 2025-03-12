using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UserManagement.DTOs;
using UserManagement.Interfaces;

namespace UserManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TokenController : BaseController
    {
        private readonly ITokenService _tokenService;
        private readonly ILogger<TokenController> _logger;

        public TokenController(ITokenService tokenService, ILogger<TokenController> logger)
        {
            _tokenService = tokenService;
            _logger = logger;
        }

        [HttpPost("revokeToken")]
        public async Task<IActionResult> RevokeToken([FromBody] RevokeToken model)
        {
            try
            {
                _logger.LogInformation("Attempting to revoke token.");

                var token = model.Token ?? Request.Cookies["refreshToken"];

                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("Token revocation failed: Token is required.");
                    return BadRequest("Token is required!");
                }

                var result = await _tokenService.RevokeTokenAsync(token);

                return result.Match(
                    Success =>
                    {
                        _logger.LogInformation("Token revoked successfully.");
                        return Ok();
                    },
                    errors =>
                    {
                        _logger.LogWarning("Token revocation failed.");
                        return Problem(errors);
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while revoking token.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while revoking the token.");
            }
        }

        [HttpGet("refreshToken")]
        public async Task<IActionResult> RefreshToken()
        {
            try
            {
                _logger.LogInformation("Attempting to refresh token.");

                var tokenRequest = new RefreshTokenRequest
                {
                    RefreshToken = Request.Cookies["refreshToken"],
                    UserDeviceId = HttpContext.Request.Headers["User-Agent"].ToString(),
                    UserIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
                };

                var result = await _tokenService.RefreshTokenAsync(tokenRequest);

                return result.Match(
                    authModel =>
                    {
                        SetRefreshTokenInCookie(authModel.RefreshToken, authModel.RefreshTokenExpiration);
                        _logger.LogInformation("Token refreshed successfully for user {UserName}.", authModel.Username);
                        return Ok(authModel);
                    },
                    errors =>
                    {
                        _logger.LogWarning("Token refresh failed.");
                        return Problem(errors);
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while refreshing token.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while refreshing the token.");
            }
        }

      
        private void SetRefreshTokenInCookie(string? refreshToken, DateTime expires)
        {
            try
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
                _logger.LogInformation("Refresh token set in cookie, expires at {Expiration}.", expires);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to set refresh token in cookie.");
            }
        }
    }
}
