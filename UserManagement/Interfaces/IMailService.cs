using Microsoft.AspNetCore.Http;

namespace UserManagement.Interfaces
{
    public interface IMailService
    {
        Task SendEmailAsync(string mailTo, string subject, string body, IList<IFormFile> attachments = null);
    }
}
