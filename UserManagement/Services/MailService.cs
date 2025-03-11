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
        private readonly IFormateService _formateService;
        public MailService(IOptions<MailSettings> mailSettings, IFormateService formateService)
        {
            _mailSettings = mailSettings.Value;
            _formateService = formateService;
        }

        public async Task<ErrorOr<bool>> SendEmailAsync(string mailTo, string subject, string body, IList<IFormFile> attachments = null)
        {
            try
            {
                if (string.IsNullOrEmpty(mailTo) || !mailTo.Contains('@'))
                    return MailErrors.InvalidEmail;

                var email = new MimeMessage
                {
                    Sender = MailboxAddress.Parse(_mailSettings.Username),
                    Subject = subject
                };

                email.To.Add(MailboxAddress.Parse(mailTo));

                var builder = new BodyBuilder();

                // Create HTML email template
                var htmlBody = _formateService.GenerateHtmlBody(_mailSettings.DisplayName ?? "No Name", body);
                if(htmlBody.IsError)
                {
                    return Error.Failure(description: "can not generate Html Body");
                }

                builder.HtmlBody = htmlBody.Value;

                // Handle attachments
                if (attachments != null && attachments.Any())
                {
                    foreach (var file in attachments)
                    {
                        if (file.Length > 0)
                        {
                            using var ms = new MemoryStream();
                            await file.CopyToAsync(ms);
                            builder.Attachments.Add(file.FileName, ms.ToArray(), ContentType.Parse(file.ContentType));
                        }
                        else
                        {
                            return MailErrors.AttachmentError;
                        }
                    }
                }

                email.From.Add(new MailboxAddress(_mailSettings.DisplayName, _mailSettings.Username));
                email.Body = builder.ToMessageBody();

                using var smtp = new SmtpClient();
                await smtp.ConnectAsync(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync(_mailSettings.Username, _mailSettings.Password);
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);

                return true;
            }
            catch (Exception ex)
            {
                // Consider logging the exception here
                return MailErrors.FailedToSendEmail;
            }
        }
    }
}