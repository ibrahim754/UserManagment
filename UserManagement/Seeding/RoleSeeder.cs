using Microsoft.Extensions.Logging;
using UserManagement.Constans;
using UserManagement.Interfaces;

namespace UserManagement.Seeding
{
    internal class RoleSeeder : IDataSeeder
    {
        private readonly IRoleService _roleService;
        private readonly ILogger<RoleSeeder> _logger;
        public RoleSeeder(IRoleService roleService,ILogger<RoleSeeder> logger )
        { 
            _roleService = roleService;
            _logger = logger;
        }
        public int OrderOfExecution => 1;
        public async Task SeedAsync()
        {
            _logger.LogInformation("Start Seeding the defaultRoles");

            foreach (var role in Enum.GetNames(typeof(DefaultRoles)))
            {
                try
                {
                    _logger.LogInformation("Start Seeding the role {role-name}", role);
                    var result = await _roleService.AddNewRoleAsync(role );
                    if (result.IsError)
                    {
                        _logger.LogWarning("Could not seed the role {role-name} due to {error}",
                            role.ToString(), result.Errors.FirstOrDefault().Description);
                        continue;
                    }
                    _logger.LogInformation("Role {role-name} seeded succfully", role );
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Could not  Seed the role {role-name} due to exception {ex-decription}", role,ex.Message);
                }
            }
        }
    }
}
