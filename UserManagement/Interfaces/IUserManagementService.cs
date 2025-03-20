using ErrorOr;
using UserManagement.DTOs;
using UserManagement.Models;

public interface IUserManagementService
{
    Task<ErrorOr<IReadOnlyCollection<User>>> BrowseAsync();
    Task<ErrorOr<bool>> BlockUser(string userIdentifier);
    Task<ErrorOr<bool>> ActivateUser(string userIdentifier);
    Task<ErrorOr<string>> ChangePasswordAsync(ChangePasswordRequest changePassword);
    Task<ErrorOr<string>> AddRoleToUserAsync(AddRoleModel model);


}