
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.CodeAnalysis.Options;
using Microsoft.Extensions.Options;
using MimeKit;
using System.Security.AccessControl;

namespace Web
{
    public class MailService : IMailService
    {
        private readonly MailSettings _mailSettings;
        public MailService(IOptions<MailSettings>mailSettings)
        {
            _mailSettings = mailSettings.Value;
        }
        public async Task SendEmailAsync(string mailTo, string subject, string body, IList<IFormFile> attachments = null)
        {

            var email = new MimeMessage
            {
                Sender = MailboxAddress.Parse(_mailSettings.Username),
                Subject = subject
            };
            email.To.Add (MailboxAddress.Parse(mailTo));

            var builder = new BodyBuilder();
            if (attachments != null)
            {
                byte[] fileBytes;
                foreach (var attachment in attachments)
                {
                    if (attachment.Length > 0)
                    {
                        using var ms = new MemoryStream();
                        attachment.CopyTo(ms);
                        fileBytes = ms.ToArray();

                        builder.Attachments.Add(attachment.FileName, fileBytes, ContentType.Parse(attachment.ContentType));
                    }
                }
            }
            builder.HtmlBody = body;
            email.Body = builder.ToMessageBody();
            email.From.Add(new MailboxAddress(_mailSettings.DisplayName,_mailSettings.Username));
            
            using var smtp = new SmtpClient();
            smtp.Connect(_mailSettings.Host, _mailSettings.Port,SecureSocketOptions.SslOnConnect);
            smtp.Authenticate(_mailSettings.Username, _mailSettings.Password);
            Console.WriteLine($"__________\n\nEmail:{_mailSettings.Username}\n\nPassword:{_mailSettings.Password}\n\n");
            await smtp.SendAsync(email);
            
            smtp.Disconnect(true);
        }
    }
}
