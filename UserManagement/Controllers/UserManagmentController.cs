using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Interfaces;
using UserManagement.DTOs;
using Microsoft.Extensions.Logging;
using UserManagement.Controllers;
using UserManagement.Services;

namespace Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserManagmentController : BaseController
    {
        private readonly IUserManagementService _userManagmentService;
        private readonly ILogger<UserManagmentController> _logger;

        public UserManagmentController(IUserManagementService  userManagmentService, ILogger<UserManagmentController> logger)
        {
            _userManagmentService = userManagmentService;
            _logger = logger;
        }

        [HttpPost("changePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest model)
        {
            if (string.IsNullOrWhiteSpace(model.userIdentifier) ||
                string.IsNullOrWhiteSpace(model.CurrentPassword) ||
                string.IsNullOrWhiteSpace(model.NewPassword))
            {
                _logger.LogWarning("Password change failed: User ID, current password, and new password are required.");
                return BadRequest("User ID, current password, and new password are required!");
            }

            try
            {
                _logger.LogInformation("Attempting to change password for user {userIdentifier}", model.userIdentifier);

                var result = await _userManagmentService.ChangePasswordAsync(model);

                return result.Match(
                    success =>
                    {
                        _logger.LogInformation("Password changed successfully for user {userIdentifier}", model.userIdentifier);
                        return Ok("Password changed successfully");
                    },
                    errors =>
                    {
                        _logger.LogWarning("Password change failed for user {userIdentifier}", model.userIdentifier);
                        return Problem(errors);
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while changing password for user {userIdentifier}", model.userIdentifier);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred during password change.");
            }
        }
        [HttpGet("browse")]
        public async Task<IActionResult> BrowseUsers()
        {
            try
            {
                var result = await _userManagmentService.BrowseAsync();
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
                var result = await _userManagmentService.BlockUser(userId);
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
        [HttpPost("addRole")]
        public async Task<IActionResult> AddRoleAsync([FromBody] AddRoleModel model)
        {
            try
            {
                _logger.LogInformation("Attempting to add role {Role} to user {userIdentifier}.", model.Role, model.userIdentifier);

                var result = await _userManagmentService.AddRoleToUserAsync(model);

                return result.Match(
                    _ =>
                    {
                        _logger.LogInformation("Role {Role} added successfully to user {userIdentifier}.", model.Role, model.userIdentifier);
                        return Ok($"Role '{model.Role}' added to user with ID '{model.userIdentifier}'.");
                    },
                    errors =>
                    {
                        _logger.LogWarning("Failed to add role {Role} to user {userIdentifier}. Errors: {Errors}", model.Role, model.userIdentifier, errors);
                        return Problem(errors);
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding role {Role} to user {userIdentifier}.", model.Role, model.userIdentifier);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while adding the role.");
            }
        }

    }
}
