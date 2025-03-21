using ErrorOr;
using UserManagement.DTOs;
using UserManagement.Models;

namespace UserManagement.Interfaces
{
    public interface IRegistrationService
    {
        Task<ErrorOr<Guid>> RegisterAsync(RegisterModel model, UserAgent? userAgent = null, List<string>? roles = null);
        Task<ErrorOr<AuthModel>> ConfirmRegisterAsync(ConfirmationUserDto confirmationUser, UserAgent? userAgent = null);
        Task<ErrorOr<AuthModel>> CreateUserAsync(User user, string password, List<string> roles, UserAgent? userAgent = null);
    }
}
