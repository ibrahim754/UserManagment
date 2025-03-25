using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UserManagement.DTOs;
using Microsoft.Extensions.Logging;
using UserManagement.Controllers;

namespace Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserManagmentController : BaseController
    {
        private readonly IUserManagementService _userManagmentService;
        private readonly ILogger<UserManagmentController> _logger;

        public UserManagmentController(IUserManagementService userManagmentService, ILogger<UserManagmentController> logger)
        {
            _userManagmentService = userManagmentService;
            _logger = logger;
        }

        [HttpPost("resetPassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest model)
        {
            if (string.IsNullOrWhiteSpace(model.userIdentifier) ||
                string.IsNullOrWhiteSpace(model.CurrentPassword) ||
                string.IsNullOrWhiteSpace(model.NewPassword))
            {
                _logger.LogWarning("Password change failed: User ID, current password, and new password are required.");
                return BadRequest("User ID, current password, and new password are required!");
            }


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
        [HttpGet("browse")]
        public async Task<IActionResult> BrowseUsers()
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
        [HttpGet("BlockUser")]
        public async Task<IActionResult> BlockUser(string userIdentifier)

        {
            var result = await _userManagmentService.BlockUser(userIdentifier);
            return result.Match(
                success =>
                {
                    _logger.LogInformation("Blocked User With identifier {userIdentifier} succfully", userIdentifier);
                    return Ok(result.Value);
                },
                errors =>
                {
                    _logger.LogWarning("Could not block User With identifier {userIdentifier}, Due to {Errors} ", userIdentifier, errors);
                    return Problem(errors);
                }
                );

        }
        [HttpGet("activateUser")]
        public async Task<IActionResult> activateUserAsync(string userIdentifier)
        {

            var result = await _userManagmentService.ActivateUser(userIdentifier);
            return result.Match(
                success =>
                {
                    _logger.LogInformation("Activate User With identifier {userIdentifier} succfully", userIdentifier);
                    return Ok(result.Value);
                },
                errors =>
                {
                    _logger.LogWarning("Could not Activate User With identifier {userIdentifier}, Due to {Errors} ", userIdentifier, errors);
                    return Problem(errors);
                }
                );

        }
        [HttpPost("addRole")]
        public async Task<IActionResult> AddRoleAsync([FromBody] AddRoleModel model)
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


    }
}
