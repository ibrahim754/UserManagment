using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UserManagement.DTOs;
using Microsoft.Extensions.Logging;
using UserManagement.Controllers;

namespace Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserManagmentController(
        IUserManagementService userManagmentService,
        ILogger<UserManagmentController> logger)
        : BaseController
    {
        [HttpPost("resetPassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest model)
        {
            if (string.IsNullOrWhiteSpace(model.userIdentifier) ||
                string.IsNullOrWhiteSpace(model.CurrentPassword) ||
                string.IsNullOrWhiteSpace(model.NewPassword))
            {
                logger.LogWarning("Password change failed: User ID, current password, and new password are required.");
                return BadRequest("User ID, current password, and new password are required!");
            }


            logger.LogInformation("Attempting to change password for user {userIdentifier}", model.userIdentifier);

            var result = await userManagmentService.ChangePasswordAsync(model);

            return result.Match(
                success =>
                {
                    logger.LogInformation("Password changed successfully for user {userIdentifier}", model.userIdentifier);
                    return Ok("Password changed successfully");
                },
                errors =>
                {
                    logger.LogWarning("Password change failed for user {userIdentifier}", model.userIdentifier);
                    return Problem(errors);
                });

        }
        [HttpGet("browse")]
        public async Task<IActionResult> BrowseUsers()
        {
            var result = await userManagmentService.BrowseAsync();
            return result.Match(
                success =>
                {
                    logger.LogInformation("Browse Users succeded");
                    return Ok(result.Value);
                },
                errors =>
                {
                    logger.LogWarning("Could not browse users");
                    return Problem(errors);
                }
                );

        }
        [HttpGet("BlockUser")]
        public async Task<IActionResult> BlockUser(string userIdentifier)

        {
            var result = await userManagmentService.BlockUser(userIdentifier);
            return result.Match(
                success =>
                {
                    logger.LogInformation("Blocked User With identifier {userIdentifier} succfully", userIdentifier);
                    return Ok(result.Value);
                },
                errors =>
                {
                    logger.LogWarning("Could not block User With identifier {userIdentifier}, Due to {Errors} ", userIdentifier, errors);
                    return Problem(errors);
                }
                );

        }
        [HttpGet("activateUser")]
        public async Task<IActionResult> activateUserAsync(string userIdentifier)
        {

            var result = await userManagmentService.ActivateUser(userIdentifier);
            return result.Match(
                success =>
                {
                    logger.LogInformation("Activate User With identifier {userIdentifier} succfully", userIdentifier);
                    return Ok(result.Value);
                },
                errors =>
                {
                    logger.LogWarning("Could not Activate User With identifier {userIdentifier}, Due to {Errors} ", userIdentifier, errors);
                    return Problem(errors);
                }
                );

        }
        [HttpPost("addRole")]
        public async Task<IActionResult> AddRoleAsync([FromBody] AddRoleModel model)
        {
            logger.LogInformation("Attempting to add role {Role} to user {userIdentifier}.", model.Role, model.userIdentifier);

            var result = await userManagmentService.AddRoleToUserAsync(model);

            return result.Match(
                _ =>
                {
                    logger.LogInformation("Role {Role} added successfully to user {userIdentifier}.", model.Role, model.userIdentifier);
                    return Ok($"Role '{model.Role}' added to user with ID '{model.userIdentifier}'.");
                },
                errors =>
                {
                    logger.LogWarning("Failed to add role {Role} to user {userIdentifier}. Errors: {Errors}", model.Role, model.userIdentifier, errors);
                    return Problem(errors);
                });

        }


    }
}
