using ErrorOr;
using UserManagement.DTOs;

namespace UserManagement.Interfaces
{
    public interface IAuthService
    {
        Task<ErrorOr<AuthModel>> LogInAsync(TokenRequestModel model, UserAgent userAgent);
        Task<ErrorOr<bool>> LogOutAsync(string userIdentifier);
    }
}
