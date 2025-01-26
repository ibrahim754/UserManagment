using ErrorOr;
using Microsoft.AspNetCore.Http;

namespace UserManagement.Interfaces
{
    public interface IMailService
    {
        Task<ErrorOr<bool>> SendEmailAsync(string mailTo, string subject, string body, IList<IFormFile> attachments = null);

    }
}
