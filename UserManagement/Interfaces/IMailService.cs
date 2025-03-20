using ErrorOr;
using Microsoft.AspNetCore.Http;
using UserManagement.DTOs;

namespace UserManagement.Interfaces
{
    public interface IMailService
    {
        Task<ErrorOr<bool>> SendEmailAsync(MailRequestDto mailRequest);

    }
}
