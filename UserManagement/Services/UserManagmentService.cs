using ErrorOr;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UserManagement.DTOs;
using UserManagement.Errors;
using UserManagement.Interfaces;
using UserManagement.Models;

namespace UserManagement.Services
{
    public class UserManagementService(
        UserManager<User> userManager,
        ILogger<UserManagementService> logger,
        IRoleService roleService)
        : IUserManagementService
    {
        public async Task<ErrorOr<IReadOnlyCollection<User>>> BrowseAsync()
        {
            logger.LogInformation("Fetching all users");

            var users = await userManager.Users
                .Include(e => e.RefreshTokens)
                .AsNoTracking()
                .ToListAsync();

            logger.LogInformation("Retrieved {UserCount} users from the database", users.Count);
            return users;

        }
        public async Task<ErrorOr<string>> ChangePasswordAsync(ChangePasswordRequest changePassword)
        {

            logger.LogInformation("Change password request for user: {userIdentifier}", changePassword.userIdentifier);

            var userExist = await ExistUser(changePassword.userIdentifier);
            if (userExist.IsError)
            {
                logger.LogWarning("User with identifier {identifier} is not exist ", changePassword.userIdentifier);
                return userExist.Errors;
            }

            var user = userExist.Value;
            var checkPassword = await userManager.CheckPasswordAsync(user, changePassword.CurrentPassword);
            if (!checkPassword)
            {
                logger.LogWarning("Password change attempt with incorrect current password for user: {userIdentifier}", changePassword.userIdentifier);
                return UserErrors.IncorrectPassword;
            }

            var result = await userManager.ChangePasswordAsync(user, changePassword.CurrentPassword, changePassword.NewPassword); 
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                logger.LogError("Failed to change password for user: {userIdentifier}. Errors: {Errors}", changePassword.userIdentifier, errors);
                return UserErrors.ChangePasswordFailed;
            }
            var result2 = await userManager.UpdateSecurityStampAsync(user);

            logger.LogInformation("Password changed successfully for user: {userIdentifier}", changePassword.userIdentifier);
            return "Changed Successfully";

        }
        public async Task<ErrorOr<bool>> ActivateUser(string userIdentifier)
        {

            var userExist = await ExistUser(userIdentifier);
            if (userExist.IsError)
            {
                logger.LogWarning("User with identifier {identifier} does not exist", userIdentifier);
                return userExist.Errors;
            }

            var user = userExist.Value;
            // Remove lockout by setting LockoutEnd to null.
            var result = await userManager.SetLockoutEndDateAsync(user, null);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                logger.LogWarning("Could not activate the user due to: {errors}", errors);
                return Error.Validation(description: errors);
            }
            return true;

        }

        public async Task<ErrorOr<bool>> BlockUser(string userIdentifier)
        {

            var userExist = await ExistUser(userIdentifier);
            if (userExist.IsError)
            {
                logger.LogWarning("User with identifier {identifier} is not exist ", userIdentifier);
                return userExist.Errors;
            }
            var user = userExist.Value;
            await userManager.SetLockoutEnabledAsync(user, true);
            var result = await userManager.SetLockoutEndDateAsync(user, DateTime.UtcNow.AddMinutes(10));


            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                logger.LogWarning("Could not block the user due to {errors}", errors);
                return Error.Validation(description: errors);
            }
            return true;

        }
        public async Task<ErrorOr<string>> AddRoleToUserAsync(AddRoleModel model)
        {

            var userExist = await ExistUser(model.userIdentifier);
            if (userExist.IsError)
            {
                logger.LogWarning("User with identifier {identifier} is not exist ", model.userIdentifier);
                return userExist.Errors;
            }

            var roleExist = await roleService.IsExistAsync(model.Role);
            if (roleExist.IsError)
            {
                logger.LogWarning("An Error when Adding role {roleName} to the userIdentifier {UserId} , because {Errors}", model.Role, model.userIdentifier, roleExist.Errors);
                return Error.Validation(description: "Role Is not Exist");
            }

            var user = userExist.Value;
            if (await userManager.IsInRoleAsync(user, model.Role))
            {
                logger.LogWarning("User {userIdentifier} is already in role {Role}", model.userIdentifier, model.Role);
                return Error.Conflict(description: "User already assigned to this role");
            }

            var result = await userManager.AddToRoleAsync(user, model.Role);
            if (result.Succeeded)
            {
                logger.LogInformation("Role {Role} successfully added to user {userIdentifier}", model.Role, model.userIdentifier);
                return string.Empty;
            }

            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            logger.LogError("Failed to add role {Role} to user {userIdentifier}. Errors: {Errors}", model.Role, model.userIdentifier, errors);
            return Error.Failure(description: errors);

        }
        public async Task<ErrorOr<User>> ExistUser(string userIdentifier)
        {

            var user = new User();
            if (Guid.TryParse(userIdentifier, out Guid _))
            {
                user = await userManager.FindByIdAsync(userIdentifier);
            }
            else if (userIdentifier.Contains("@"))
            {
                user = await userManager.FindByEmailAsync(userIdentifier);
            }
            else
            {
                user = await userManager.FindByNameAsync(userIdentifier);
            }
            if (user is null)
            {
                logger.LogWarning($"User with userIdentifier {userIdentifier} is not exist");
                return UserErrors.UserNotFound;
            }
            logger.LogInformation($"User with userIdentifier: {userIdentifier} was found");
            return user;

        }

    }
}