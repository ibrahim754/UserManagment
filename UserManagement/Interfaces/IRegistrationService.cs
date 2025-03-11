using ErrorOr;
using UserManagement.DTOs;

namespace UserManagement.Interfaces
{
    public interface IRegistrationService
    {
        Task<ErrorOr<Guid>> RegisterAsync(RegisterModel model, UserAgent userAgent);
        Task<ErrorOr<AuthModel>> ConfirmRegisterAsync(ConfirmationUserDto confirmationUser, UserAgent userAgent);

    }
}
