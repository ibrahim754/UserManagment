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
    public class UserManagementService : IUserManagementService
    {
        private readonly UserManager<User> _userManager;
        private readonly ILogger<UserManagementService> _logger;
        private readonly IRoleService _roleService;

        public UserManagementService(
            UserManager<User> userManager,
            ILogger<UserManagementService> logger,
            IRoleService roleService
            )
        {
            _userManager = userManager;
            _logger = logger;
            _roleService = roleService;
        }

        public async Task<ErrorOr<IReadOnlyCollection<User>>> BrowseAsync()
        {

            _logger.LogInformation("Fetching all users");

            var users = await _userManager.Users
                .Include(e => e.RefreshTokens)
                .AsNoTracking()
                .ToListAsync();

            _logger.LogInformation("Retrieved {UserCount} users from the database", users.Count);
            return users;

        }
        public async Task<ErrorOr<string>> ChangePasswordAsync(ChangePasswordRequest changePassword)
        {

            _logger.LogInformation("Change password request for user: {userIdentifier}", changePassword.userIdentifier);

            var userExist = await ExistUser(changePassword.userIdentifier);
            if (userExist.IsError)
            {
                _logger.LogWarning("User with identifier {identifier} is not exist ", changePassword.userIdentifier);
                return userExist.Errors;
            }
            var user = userExist.Value;

            var checkPassword = await _userManager.CheckPasswordAsync(user, changePassword.CurrentPassword);
            if (!checkPassword)
            {
                _logger.LogWarning("Password change attempt with incorrect current password for user: {userIdentifier}", changePassword.userIdentifier);
                return UserErrors.IncorrectPassword;
            }

            var result = await _userManager.ChangePasswordAsync(user, changePassword.CurrentPassword, changePassword.NewPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError("Failed to change password for user: {userIdentifier}. Errors: {Errors}", changePassword.userIdentifier, errors);
                return UserErrors.ChangePasswordFailed;
            }
            var result2 = await _userManager.UpdateSecurityStampAsync(user);

            _logger.LogInformation("Password changed successfully for user: {userIdentifier}", changePassword.userIdentifier);
            return "Changed Successfully";

        }
        public async Task<ErrorOr<bool>> ActivateUser(string userIdentifier)
        {

            var userExist = await ExistUser(userIdentifier);
            if (userExist.IsError)
            {
                _logger.LogWarning("User with identifier {identifier} does not exist", userIdentifier);
                return userExist.Errors;
            }

            var user = userExist.Value;
            // Remove lockout by setting LockoutEnd to null.
            var result = await _userManager.SetLockoutEndDateAsync(user, null);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogWarning("Could not activate the user due to: {errors}", errors);
                return Error.Validation(description: errors);
            }
            return true;

        }

        public async Task<ErrorOr<bool>> BlockUser(string userIdentifier)
        {

            var userExist = await ExistUser(userIdentifier);
            if (userExist.IsError)
            {
                _logger.LogWarning("User with identifier {identifier} is not exist ", userIdentifier);
                return userExist.Errors;
            }
            var user = userExist.Value;
            await _userManager.SetLockoutEnabledAsync(user, true);
            var result = await _userManager.SetLockoutEndDateAsync(user, DateTime.UtcNow.AddMinutes(10));


            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogWarning("Could not block the user due to {errors}", errors);
                return Error.Validation(description: errors);
            }
            return true;

        }
        public async Task<ErrorOr<string>> AddRoleToUserAsync(AddRoleModel model)
        {

            var userExist = await ExistUser(model.userIdentifier);
            if (userExist.IsError)
            {
                _logger.LogWarning("User with identifier {identifier} is not exist ", model.userIdentifier);
                return userExist.Errors;
            }

            var roleExist = await _roleService.IsExistAsync(model.Role);
            if (roleExist.IsError)
            {
                _logger.LogWarning("An Error when Adding role {roleName} to the userIdentifier {UserId} , because {Errors}", model.Role, model.userIdentifier, roleExist.Errors);
                return Error.Validation(description: "Role Is not Exist");
            }

            var user = userExist.Value;
            if (await _userManager.IsInRoleAsync(user, model.Role))
            {
                _logger.LogWarning("User {userIdentifier} is already in role {Role}", model.userIdentifier, model.Role);
                return Error.Conflict(description: "User already assigned to this role");
            }

            var result = await _userManager.AddToRoleAsync(user, model.Role);
            if (result.Succeeded)
            {
                _logger.LogInformation("Role {Role} successfully added to user {userIdentifier}", model.Role, model.userIdentifier);
                return string.Empty;
            }

            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogError("Failed to add role {Role} to user {userIdentifier}. Errors: {Errors}", model.Role, model.userIdentifier, errors);
            return Error.Failure(description: errors);

        }
        public async Task<ErrorOr<User>> ExistUser(string userIdentifier)
        {

            var user = new User();
            if (Guid.TryParse(userIdentifier, out Guid _))
            {
                user = await _userManager.FindByIdAsync(userIdentifier);
            }
            else if (userIdentifier.Contains("@"))
            {
                user = await _userManager.FindByEmailAsync(userIdentifier);
            }
            else
            {
                user = await _userManager.FindByNameAsync(userIdentifier);
            }
            if (user is null)
            {
                _logger.LogWarning($"User with userIdentifier {userIdentifier} is not exist");
                return UserErrors.UserNotFound;
            }
            _logger.LogInformation($"User with userIdentifier: {userIdentifier} was found");
            return user;


        }

    }
}