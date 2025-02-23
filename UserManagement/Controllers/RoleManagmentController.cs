using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UserManagement.Interfaces;
using UserManagement.DTOs;

namespace UserManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleManagmentController : BaseController
    {
        private readonly IRoleService _roleService;
        private readonly ILogger<RoleManagmentController> _logger;

        public RoleManagmentController(IRoleService roleService, ILogger<RoleManagmentController> logger)
        {
            _roleService = roleService;
            _logger = logger;
        }

        [HttpPost("createRole")]
        public async Task<IActionResult> CreateRole(string roleName)
        {
            try
            {
                _logger.LogInformation("Attempting to create role with name {RoleName}.", roleName);

                var result = await _roleService.AddNewRoleAsync(roleName);
                if (result.IsError)
                {
                    _logger.LogWarning("Failed to create role: {RoleName}. Errors: {Errors}", roleName, result.Errors);
                    return Problem(result.Errors);
                }

                _logger.LogInformation("Role created successfully with name {RoleName}.", roleName);
                return Ok($"Role with name {roleName} was added successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating role with name {RoleName}.", roleName);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while creating the role.");
            }
        }

        [HttpPost("addRole")]
        public async Task<IActionResult> AddRoleAsync([FromBody] AddRoleModel model)
        {
            try
            {
                _logger.LogInformation("Attempting to add role {Role} to user {UserId}.", model.Role, model.UserId);

                var result = await _roleService.AddRoleToUserAsync(model);

                return result.Match(
                    _ =>
                    {
                        _logger.LogInformation("Role {Role} added successfully to user {UserId}.", model.Role, model.UserId);
                        return Ok($"Role '{model.Role}' added to user with ID '{model.UserId}'.");
                    },
                    errors =>
                    {
                        _logger.LogWarning("Failed to add role {Role} to user {UserId}. Errors: {Errors}", model.Role, model.UserId, errors);
                        return Problem(errors);
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding role {Role} to user {UserId}.", model.Role, model.UserId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while adding the role.");
            }
        }
        [HttpDelete("deleteRole")]
        public async Task<IActionResult> DeleteRoleAsync(string roleName)
        {
            try
            {
                if (string.IsNullOrEmpty(roleName))
                {
                    _logger.LogWarning("Role Name Can not be empty");
                    return BadRequest("Role name can not be empty");
                }
                var result = await _roleService.DeleteRoleAsync(roleName);
                return result.Match(
                  _ =>
                  {
                      _logger.LogInformation("role {roleName} is deleted succfully", roleName);
                      return Ok($"role {roleName} was deleted ");
                  },
                  errors =>
                  {
                      _logger.LogWarning("Failed to delete role {Role} }", roleName);
                      return Problem(errors);
                  });

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting role {Role} ", roleName);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while deleting the role.");

            }
        }
        [HttpGet("browse")]
        public async Task<IActionResult> browseAsync()
        {
            try
            {
                
                var result = await _roleService.BrowseAsync();
                return result.Match(
                  _ =>
                  {
                      _logger.LogInformation("Browsing Roles");
                      return Ok(_);
                  },
                  errors =>
                  {
                      _logger.LogWarning("Failed to Broswing the roles ");
                      return Problem(errors);
                  });

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while browsing the roles");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while Browsing the roles");

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
                _logger.LogInformation("Refresh token set in cookie, expires at {Expiration}.", expires);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to set refresh token in cookie.");
            }
        }
    }
}
