using ErrorOr;
using Microsoft.Extensions.Logging;
using UserManagement.Constans;
using UserManagement.DTOs;
using UserManagement.Interfaces;
using UserManagement.Models;

namespace UserManagement.Seeding
{
    public class UsersSeeder(
        IUserManagementService userManagementService,
        ILogger<UsersSeeder> logger,
        IRegistrationService registrationService)
        : IDataSeeder
    {
        public int OrderOfExecution => 2;  

        public async Task SeedAsync()
        {
            logger.LogInformation("Seeding users...");

            var usersToSeed = SeedUsersData.Users;

            foreach (var user in usersToSeed)
            {

                var existingUserResult = await userManagementService.ExistUser(user.user.UserName ?? "");

                if (existingUserResult.IsError)
                {
                    var roles = Enum.GetNames(typeof(DefaultRoles)).ToList();
                    logger.LogInformation("User {UserName} does not exist. Creating...", user.user.UserName);
                    var createUserResult = await registrationService.CreateUserAsync(user.user, user.password, roles);
                    if (createUserResult.IsError)
                    {
                        logger.LogError("Failed to create user {UserName}: {Error}, error from register service", user.user.UserName, createUserResult.FirstError.Description);
                    }
                    else
                    {
                        logger.LogInformation("User {UserName} created successfully.", user.user.UserName);
                    }

                }
                else
                {
                    logger.LogWarning("User {user-name} is already exist", user.user.UserName);
                }
            }
        }
    }
}