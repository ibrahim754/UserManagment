using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace UserManagement.DAL
{
    /*
    public class UserManagmentDbContextactory : IDesignTimeDbContextFactory<UserManagmentDbContext>
    {
        public UserManagmentDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
            var optionsBuilder = new DbContextOptionsBuilder<UserManagmentDbContext>();
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            optionsBuilder.UseSqlServer(connectionString);  
            return new UserManagmentDbContext(optionsBuilder.Options);
        }
    }
    */
}
