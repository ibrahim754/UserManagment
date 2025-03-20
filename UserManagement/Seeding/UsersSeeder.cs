using ErrorOr;
using Microsoft.Extensions.Logging;
using UserManagement.Constans;
using UserManagement.DTOs;
using UserManagement.Interfaces;
using UserManagement.Models;

namespace UserManagement.Seeding
{
    public class UsersSeeder : IDataSeeder
    {
        private readonly IUserManagementService _userManagementService;
        private readonly ILogger<UsersSeeder> _logger;
        private readonly IRegistrationService _registrationService; 
        public UsersSeeder(IUserManagementService userManagementService, ILogger<UsersSeeder> logger, IRegistrationService registrationService)
        {
            _userManagementService = userManagementService;
            _logger = logger;
            _registrationService = registrationService;
        }

        public int OrderOfExecution => 2;  

        public async Task SeedAsync()
        {
            _logger.LogInformation("Seeding users...");

            var usersToSeed = SeedUsersData.Users;

            foreach (var user in usersToSeed)
            {
                var existingUserResult = await _userManagementService.ExistUser(user.user.UserName);

                if (existingUserResult.IsError)
                {
                    _logger.LogInformation("User {UserName} does not exist. Creating...", user.user.UserName);
                    // Create the user
                    var createUserResult = await _registrationService.CreateUserAsync(user.user, user.password);
                    if (createUserResult.IsError)
                    {
                        _logger.LogError("Failed to create user {UserName}: {Error}", user.user.UserName, createUserResult.FirstError.Description);
                    }
                    else
                    {
                        _logger.LogInformation("User {UserName} created successfully.", user.user.UserName);
                    }
                }
                else
                {
                    _logger.LogInformation("User {UserName} already exists. Updating password...", user.user.UserName);
                    // Update the password
                    var changePasswordResult = await _userManagementService.ChangePasswordAsync(new ChangePasswordRequest
                    {
                        userIdentifier = user.user.UserName,
                        NewPassword = user.password
                    });

                    if (changePasswordResult.IsError)
                    {
                        _logger.LogError("Failed to update password for user {UserName}: {Error}", user.user.UserName, changePasswordResult.FirstError.Description);
                    }
                    else
                    {
                        _logger.LogInformation("Password updated successfully for user {UserName}.", user.user.UserName);
                    }
                }
            }
        }
    }
}