using ErrorOr;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using UserManagement.DTOs;
using UserManagement.Interfaces;
using UserManagement.Models;

namespace UserManagement.Services
{
    public class RoleService : IRoleService
    {
        private readonly RoleManager<IdentityRole> _roleManager;
         private readonly ILogger<RoleService> _logger;
        
        public RoleService(RoleManager<IdentityRole> roleManager, ILogger<RoleService> logger  )
        {
            _roleManager = roleManager;
             _logger = logger;
          
        }

       

        public async Task<ErrorOr<bool>> AddNewRoleAsync(string roleName)
        {
            try
            {
                if (string.IsNullOrEmpty(roleName))
                {
                    _logger.LogWarning("Role name cannot be empty.");
                    return Error.Unexpected(description: "Role name cannot be empty");
                }

                _logger.LogInformation("Checking if role {RoleName} already exists.", roleName);
                if (await _roleManager.RoleExistsAsync(roleName))
                {
                    _logger.LogWarning("Role {RoleName} already exists.", roleName);
                    return Error.Conflict(description: "Role Name already exists");
                }

                // Create a new IdentityRole with a ConcurrencyStamp
                var identityRole = new IdentityRole(roleName)
                {
                    ConcurrencyStamp = Guid.NewGuid().ToString() // Set a unique ConcurrencyStamp
                };

                var result = await _roleManager
                                        .CreateAsync(identityRole);
                if (result.Succeeded)
                {
                    _logger.LogInformation("Role {RoleName} created successfully.", roleName);
                    return true;
                }

                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError("Failed to create role {RoleName}. Errors: {Errors}", roleName, errors);
                return Error.Failure(description: "Cannot add such a role");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while creating role {RoleName}", roleName);
                return Error.Failure(description: "Something went wrong while creating role");
            }
        }

        public async Task<ErrorOr<bool>> DeleteRoleAsync(string RoleName)
        {
            try
            {
                if (string.IsNullOrEmpty(RoleName))
                {
                    _logger.LogWarning("RoleName Can not be Empty");
                    return Error.Unexpected("Role name cannot be empty");
                }
                _logger.LogInformation("Deleting Role With name {roleName}", RoleName);
                var role = await _roleManager.
                                FindByNameAsync(RoleName);
                if (role == null)
                {
                    _logger.LogWarning("Role {RoleName} is not exist", RoleName);
                    return Error.NotFound("Role With Name {roleName} Not Found", RoleName);
                }
                var result = await _roleManager.DeleteAsync(role);
                if (result.Succeeded)
                {
                    _logger.LogInformation("Role {roleName} was deleted succfully", RoleName);
                    return true;
                }

                _logger.LogError("failed to delete the role {RoleName}", RoleName);
                return Error.Failure(description: $"Something went wrong while deleting the role {RoleName}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Something went wrong while deleting the role {RoleName}", RoleName);
                return Error.Failure(description: $"Something went wrong while deleting the role {RoleName}");
            }
        }

        public async Task<ErrorOr<IReadOnlyCollection<IdentityRole>>> BrowseAsync()
        {
            try
            {
                _logger.LogInformation("Browsing Roles");
                var roles = await _roleManager.Roles
                    .AsNoTracking()
                    .ToListAsync();
                return roles;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Something went wrong while Browsing the roles");
                return Error.Failure(description: "Something went wrong while Browsing the roles");

            }
        }
    }
}
