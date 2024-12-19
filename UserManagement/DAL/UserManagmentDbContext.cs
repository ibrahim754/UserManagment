using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UserManagement.Extensions;
using UserManagement.Models;

namespace UserManagement.DAL
{
    public class UserManagmentDbContext : IdentityDbContext<User>
    {
        public UserManagmentDbContext(DbContextOptions<UserManagmentDbContext> options) : base(options)
        {
        }

     
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
           
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyUserConfigurations();
        }
    }
}
