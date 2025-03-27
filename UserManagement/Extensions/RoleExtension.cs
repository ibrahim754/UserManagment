using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using UserManagement.Constans;

namespace UserManagement.Extensions
{
    internal static class RoleExtension
    {
        public static async Task AddPermissionClaim(this RoleManager<IdentityRole> roleManager, IdentityRole role )
        {
            var allClaims = await roleManager.GetClaimsAsync(role);
            List<string> allPermissions = [];
            allPermissions.AddRange(from object? permission in Enum.GetValues((typeof(UserPermissions))) select ((Enum)permission).ToPermissionString());

            foreach (var permission in allPermissions)
            {
                if (!allClaims.Any(a => a.Type == "Permission" && a.Value == permission))
                {
                    await roleManager.AddClaimAsync(role, new Claim("Permission", permission));
                }
            }
        }
    }
}
