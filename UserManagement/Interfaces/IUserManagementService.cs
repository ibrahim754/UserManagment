using ErrorOr;
using UserManagement.DTOs;
using UserManagement.Models;

public interface IUserManagementService
{
    Task<ErrorOr<IReadOnlyCollection<User>>> BrowseAsync();
    Task<ErrorOr<bool>> BlockUser(string userId);
    Task<ErrorOr<bool>> ActiveUser(string userId);
    Task<ErrorOr<string>> ChangePasswordAsync(ChangePasswordRequest changePassword);

}