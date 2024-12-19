using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserManagement.Models;

namespace UserManagement.DAL.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            // Set the table name if different from the default

            // Configure primary key
            builder.HasKey(u => u.Id);

            // Configure properties
            builder.Property(u => u.FirstName)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(u => u.LastName)
                .IsRequired()
                .HasMaxLength(50);

            // Configure relationship with RefreshTokens
            builder.HasMany(u => u.RefreshTokens)
                .WithOne(rt => rt.User)  // RefreshToken has a User
                .HasForeignKey(rt => rt.UserId)  // Foreign key in RefreshToken
                .OnDelete(DeleteBehavior.Cascade);  // Specify delete behavior
        }
    }
}
