using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Models;
using ErrorOr;
using UserManagement.Interfaces;
using UserManagement.DTOs;
using Microsoft.Extensions.Logging;
using UserManagement.Controllers;
using Microsoft.AspNetCore.Authentication;

namespace Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserManagmentController : BaseController
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserManagmentController> _logger;

        public UserManagmentController(IUserService authService, ILogger<UserManagmentController> logger)
        {
            _userService = authService;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync([FromForm]RegisterModel model)
        {
            try
            {
                _logger.LogInformation("Attempting to register user with email {Email}", model.Email);

                var userAgent = new UserAgent
                {
                    UserDevice = HttpContext.Request.Headers["User-Agent"].ToString(),
                    UserIp = HttpContext.Connection.RemoteIpAddress?.ToString()
                };
 
                var result = await _userService.RegisterAsync(model, userAgent);

                return result.Match(
                    authModel =>
                    {
                        SetRefreshTokenInCookie(authModel.RefreshToken, authModel.RefreshTokenExpiration);
                        _logger.LogInformation("User {UserName} registered successfully", authModel.Username);
                        return Ok(authModel);
                    },
                    errors =>
                    {
                        _logger.LogWarning("User registration failed for email {Email}", model.Email);
                        return Problem(errors);
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while registering user with email {Email}", model.Email);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred during registration.");
            }
        }

        [HttpPost("logIn")]
        public async Task<IActionResult> GetTokenAsync([FromBody] TokenRequestModel model)
        {
            try
            {
                _logger.LogInformation("Attempting to generate token for user with email {Email}", model.Email);

                var userAgent = new UserAgent
                {
                    UserDevice = HttpContext.Request.Headers["User-Agent"].ToString(),
                    UserIp = HttpContext.Connection.RemoteIpAddress?.ToString()
                };

                var result = await _userService.LogInAsync(model, userAgent);

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

        [HttpPost("changePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest model)
        {
            if (string.IsNullOrWhiteSpace(model.UserId) ||
                string.IsNullOrWhiteSpace(model.CurrentPassword) ||
                string.IsNullOrWhiteSpace(model.NewPassword))
            {
                _logger.LogWarning("Password change failed: User ID, current password, and new password are required.");
                return BadRequest("User ID, current password, and new password are required!");
            }

            try
            {
                _logger.LogInformation("Attempting to change password for user {UserId}", model.UserId);

                var result = await _userService.ChangePasswordAsync(model);

                return result.Match(
                    success =>
                    {
                        _logger.LogInformation("Password changed successfully for user {UserId}", model.UserId);
                        return Ok("Password changed successfully");
                    },
                    errors =>
                    {
                        _logger.LogWarning("Password change failed for user {UserId}", model.UserId);
                        return Problem(errors);
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while changing password for user {UserId}", model.UserId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred during password change.");
            }
        }
        [HttpGet("browse")]
        public async Task<IActionResult> BrowseUsers()
        {
            try
            {
                var result = await _userService.BrowseAsync();
                return result.Match(
                    success =>
                    {
                        _logger.LogInformation("Browse Users succeded");
                        return Ok(result.Value);
                    },
                    errors =>
                    {
                        _logger.LogWarning("Could not browse users");
                        return Problem(errors);
                    }
                    );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An Error Occoured While Browsing the users");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred during Browsing change.");

            }
        }
        [HttpGet("BlockUser")]
        public async Task<IActionResult> BlockUser(string userId)
        {
            try
            {
                var result = await _userService.BlockUser(userId);
                return result.Match(
                    success =>
                    {
                        _logger.LogInformation("Browse Users succeded");
                        return Ok(result.Value);
                    },
                    errors =>
                    {
                        _logger.LogWarning("Could not browse users");
                        return Problem(errors);
                    }
                    );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An Error Occoured While blocking the user");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred during blocking the user.");

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
