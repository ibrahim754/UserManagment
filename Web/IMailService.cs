namespace Web
{
    public interface IMailService
    {
        Task SendEmailAsync(string mailTo, string subject, string body,IList<IFormFile> attachments = null);
    }
}
