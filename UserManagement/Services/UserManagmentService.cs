using ErrorOr;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UserManagement.DTOs;
using UserManagement.Errors;
using UserManagement.Models;

 
public class UserManagementService : IUserManagementService
{
    private readonly UserManager<User> _userManager;
    private readonly ILogger<UserManagementService> _logger;

    public UserManagementService(
        UserManager<User> userManager,
        ILogger<UserManagementService> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<ErrorOr<IReadOnlyCollection<User>>> BrowseAsync()
    {
        try
        {
            _logger.LogInformation("Fetching all users");

            var users = await _userManager.Users.Include(e => e.RefreshTokens).AsNoTracking().ToListAsync();

            _logger.LogInformation("Retrieved {UserCount} users from the database", users.Count);
            return users;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving users.");
            return UserErrors.FetchUsersFailed;
        }
    }
    public async Task<ErrorOr<string>> ChangePasswordAsync(ChangePasswordRequest changePassword)
    {
        try
        {
            _logger.LogInformation("Change password request for user: {userIdentifier}", changePassword.userIdentifier);

            var user = await _userManager.FindByIdAsync(changePassword.userIdentifier);
            if (user is null)
            {
                _logger.LogWarning("User not found for password change: {userIdentifier}", changePassword.userIdentifier);
                return UserErrors.UserNotFound;
            }

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

            _logger.LogInformation("Password changed successfully for user: {userIdentifier}", changePassword.userIdentifier);
            return "Changed Successfully";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while changing password.");
            return UserErrors.FetchUsersFailed;
        }
    }
    public async Task<ErrorOr<bool>> ActiveUser(string userId)
    {
        try
        {

            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
            {
                _logger.LogWarning("User is Not Found");
                return UserErrors.UserNotFound;
            }
            user.LockoutEnabled = true;
            user.LockoutEnd = DateTime.UtcNow.AddMinutes(1);
            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return true;
            }
            return UserErrors.FetchUsersFailed;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An error occurred while retrieving blocking the user sith id {userId}.");
            return UserErrors.FetchUsersFailed;

        }
    }  
    public async Task<ErrorOr<bool>> BlockUser(string userIdentifier)
    {
        try
        {
            var userExist = await ExistUser(userIdentifier);
            if (userExist.IsError)
            {
                 return  userExist.Errors;
            }
            var user = userExist.Value;
            await _userManager.SetLockoutEnabledAsync(user, false);
            var result =  await _userManager.SetLockoutEndDateAsync(user, DateTime.UtcNow.AddMinutes(1));

       
            if (result.Succeeded)
            {
                return true;
            }
            return UserErrors.FetchUsersFailed;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An error occurred while retrieving blocking the user with userIdentifier {userIdentifier}.");
            return UserErrors.FetchUsersFailed;

        }
    }
    private async Task<ErrorOr<User>> ExistUser(string userIdentifier)
    {
        try
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
            _logger.LogInformation($"User with userIdentifier {userIdentifier} was found");
            return user;
        }
        catch(Exception ex) 
        {
            _logger.LogError(ex, "An error occurred while retrieving   the user with id userIdentifier {userIdentifier}.",userIdentifier);
            return UserErrors.FetchUsersFailed;
        }

    }

}