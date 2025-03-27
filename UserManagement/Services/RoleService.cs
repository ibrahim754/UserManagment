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
    public class RoleService(RoleManager<IdentityRole> roleManager, ILogger<RoleService> logger)
        : IRoleService
    {
        public async Task<ErrorOr<bool>> AddNewRoleAsync(string? roleName)
        {


            if (string.IsNullOrEmpty(roleName))
            {
                logger.LogWarning("Role name cannot be empty.");
                return Error.Unexpected(description: "Role name cannot be empty");
            }
            // must  ignore the spaces at the end and beginning 
            roleName = roleName.Trim();

            logger.LogInformation("Checking if role {roleName} already exists.", roleName);
            if (await roleManager.RoleExistsAsync(roleName))
            {
                logger.LogWarning("Role {roleName} already exists.", roleName);
                return Error.Conflict(description: "Role Name already exists");
            }

            // Create a new IdentityRole with a ConcurrencyStamp
            var identityRole = new IdentityRole(roleName)
            {
                ConcurrencyStamp = Guid.NewGuid().ToString() // Set a unique ConcurrencyStamp
            };

            var result = await roleManager
                                    .CreateAsync(identityRole);
            if (result.Succeeded)
            {
                logger.LogInformation("Role {roleName} created successfully.", roleName);
                return true;
            }

            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            logger.LogError("Failed to create role {roleName}. Errors: {Errors}", roleName, errors);
            return Error.Failure(description: "Cannot add such a role");

        }

        public async Task<ErrorOr<bool>> DeleteRoleAsync(string roleName)
        {

            roleName = roleName.Trim();
            if (string.IsNullOrEmpty(roleName))
            {
                logger.LogWarning("roleName Can not be Empty");
                return Error.Unexpected("Role name cannot be empty");
            }
            logger.LogInformation("Deleting Role With name {roleName}", roleName);
            var role = await roleManager.
                            FindByNameAsync(roleName);
            if (role == null)
            {
                logger.LogWarning("Role {roleName} is not exist", roleName);
                return Error.NotFound("Role With Name {roleName} Not Found", roleName);
            }
            var result = await roleManager.DeleteAsync(role);
            if (result.Succeeded)
            {
                logger.LogInformation("Role {roleName} was deleted successfully", roleName);
                return true;
            }

            logger.LogError("failed to delete the role {roleName}", roleName);
            return Error.Failure(description: $"Something went wrong while deleting the role {roleName}");

        }
        public async Task<ErrorOr<IdentityRole>> IsExistAsync(string roleName)
        {

            roleName = roleName.Trim();
            var result = await roleManager.FindByNameAsync(roleName);
            if (result is not null) return result;
            logger.LogWarning("The role with name {roleName} is not exist", roleName);
            return Error.NotFound($"The role with name {roleName} is not exist");

        }

        public async Task<ErrorOr<bool>> SeedRoleClaimsAsync(string roleName, List<string>? claims)
        {
            var role = await IsExistAsync(roleName);
            if (role.IsError)
            {
                logger.LogWarning("Role {roleName} is not exist", roleName);
                return Error.Failure(description:"Role {roleName} is not exist");
            }

            return true;
        }
        

        public async Task<ErrorOr<IReadOnlyCollection<IdentityRole>>> BrowseAsync()
        {

            logger.LogInformation("Browsing Roles");
            var roles = await roleManager.Roles
                .AsNoTracking()
                .ToListAsync();
            return roles;

        }
    }
}
