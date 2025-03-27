using Microsoft.Extensions.Logging;
using UserManagement.Constans;
using UserManagement.Interfaces;

namespace UserManagement.Seeding
{
    internal class RoleSeeder(IRoleService roleService, ILogger<RoleSeeder> logger) : IDataSeeder
    {
        public int OrderOfExecution => 1;
        public async Task SeedAsync()
        {
            logger.LogInformation("Start Seeding the default Roles");

            foreach (var role in Enum.GetNames(typeof(DefaultRoles)))
            {

                logger.LogInformation("Start Seeding the role {role-name}", role);
                var result = await roleService.AddNewRoleAsync(role);
                if (result.IsError)
                {
                    logger.LogWarning("Could not seed the role {role-name} due to {error}",
                        role.ToString(), result.Errors.FirstOrDefault().Description);
                    continue;
                }
                logger.LogInformation("Role {role-name} seeded successfully", role);

            }
        }
    }
}
