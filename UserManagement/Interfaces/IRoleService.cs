using ErrorOr;
using Microsoft.AspNetCore.Identity;
using UserManagement.DTOs;

namespace UserManagement.Interfaces
{
    public interface IRoleService
    {
 
        Task<ErrorOr<bool>> AddNewRoleAsync(string? roleName);
        Task<ErrorOr<bool>> DeleteRoleAsync(string roleName);
        Task<ErrorOr<IdentityRole>> IsExistAsync(string roleName);
        Task<ErrorOr<bool>> SeedRoleClaimsAsync(string roleName, List<string>? claims);
        Task<ErrorOr<IReadOnlyCollection<IdentityRole>>> BrowseAsync();
    }
}
