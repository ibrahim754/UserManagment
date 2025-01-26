using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using MimeKit;
using UserManagement.Interfaces;
using UserManagement.Errors;
using ErrorOr;
using UserManagement.Entites;

namespace UserManagement.Services
{
    public class MailService : IMailService
    {
        private readonly MailSettings _mailSettings;

        public MailService(IOptions<MailSettings> mailSettings)
        {
            _mailSettings = mailSettings.Value;
        }

        public async Task<ErrorOr<bool>> SendEmailAsync(string mailTo, string subject, string body, IList<IFormFile> attachments = null)
        {
            try
            {
                // Validate email format
                if (string.IsNullOrEmpty(mailTo) || !mailTo.Contains('@'))
                    return MailErrors.InvalidEmail;

                var email = new MimeMessage
                {
                    Sender = MailboxAddress.Parse(_mailSettings.Username),
                    Subject = subject
                };

                email.To.Add(MailboxAddress.Parse(mailTo));

                var builder = new BodyBuilder();

                // Handle attachments
                if (attachments != null && attachments.Any())
                {
                    foreach (var file in attachments)
                    {
                        if (file.Length > 0)
                        {
                            using var ms = new MemoryStream();
                            file.CopyTo(ms);
                            builder.Attachments.Add(file.FileName, ms.ToArray(), ContentType.Parse(file.ContentType));
                        }
                        else
                        {
                            return MailErrors.AttachmentError;
                        }
                    }
                }

                builder.HtmlBody = body;
                email.Body = builder.ToMessageBody();
                email.From.Add(new MailboxAddress(_mailSettings.DisplayName, _mailSettings.Username));

                using var smtp = new SmtpClient();
                await smtp.ConnectAsync(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync(_mailSettings.Username, _mailSettings.Password);

                await smtp.SendAsync(email);
                smtp.Disconnect(true);

                return true;
            }
          
            catch (Exception ex)
            {
                return MailErrors.FailedToSendEmail;
            }
        }
    }
}
