using UserManagement.Models;
using ErrorOr;
using UserManagement.DTOs;

namespace UserManagement.Interfaces
{
    public interface IUserService
    {
        Task<ErrorOr<AuthModel>> RegisterAsync(RegisterModel model, UserAgent userAgent);
        Task<ErrorOr<AuthModel>> LogInAsync(TokenRequestModel model, UserAgent userAgent);
        Task<ErrorOr<string>> ChangePasswordAsync(ChangePasswordRequest changePasswordRequest);
        Task<ErrorOr<IReadOnlyCollection<User>>> BrowseAsync();
        Task<ErrorOr<bool>> BlockUser(string UserId);

    }
}
