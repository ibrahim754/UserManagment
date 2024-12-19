using Microsoft.EntityFrameworkCore;
using UserManagement.DAL.Configurations;

namespace UserManagement.Extensions
{
    public static class ConfigurationExtension
    {
        public static ModelBuilder ApplyUserConfigurations (this ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new RefreshTokenConfiguration());

            return modelBuilder;
        }
    }
}
