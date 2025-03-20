using ErrorOr;
using Microsoft.AspNetCore.Identity;
using UserManagement.DTOs;

namespace UserManagement.Interfaces
{
    public interface IRoleService
    {
 
        Task<ErrorOr<bool>> AddNewRoleAsync(string? RoleName);
        Task<ErrorOr<bool>> DeleteRoleAsync(string RoleName);
        Task<ErrorOr<bool>> IsExistAsync(string RoleName);
        Task<ErrorOr<IReadOnlyCollection<IdentityRole>>> BrowseAsync();
    }
}
