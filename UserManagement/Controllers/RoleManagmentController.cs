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

        [HttpDelete("deleteRole")]
        public async Task<IActionResult> DeleteRoleAsync(string roleName)
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

        [HttpGet("browse")]
        public async Task<IActionResult> browseAsync()
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
        [HttpPost("checkRole")]
        public async Task<IActionResult> IsExist(string roleName)
        {
            var result = await _roleService.IsExistAsync(roleName);
            return result.Match(
              onSuccess =>
              {
                  _logger.LogInformation("Role \"{roleName}\" is exist in my DB ", roleName);
                  return Ok(onSuccess);
              },
              errors =>
              {
                  _logger.LogWarning("Role \"{roleName}\" is not exist in my DB", roleName);
                  return Problem(errors);
              });

        }
    }
}
