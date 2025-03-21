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
              
                var existingUserResult = await _userManagementService.ExistUser(user.user.UserName ?? "");

                if (existingUserResult.IsError)
                {
                    var roles = Enum.GetNames(typeof(DefaultRoles)).ToList();
                    _logger.LogInformation("User {UserName} does not exist. Creating...", user.user.UserName);
                    var createUserResult = await _registrationService.CreateUserAsync(user.user, user.password, roles);
                    if (createUserResult.IsError)
                    {
                        _logger.LogError("Failed to create user {UserName}: {Error}, error from reg service", user.user.UserName, createUserResult.FirstError.Description);
                    }
                    else
                    {
                        _logger.LogInformation("User {UserName} created successfully.", user.user.UserName);
                    }

                }
                else
                {
                    _logger.LogWarning("User {user-name} is already exist", user.user.UserName);
                }
            }
        }
    }
}