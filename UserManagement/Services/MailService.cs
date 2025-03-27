using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using MimeKit;
using UserManagement.Interfaces;
using UserManagement.Errors;
using ErrorOr;
using UserManagement.Entites;
using Microsoft.Build.Framework;
using Microsoft.Extensions.Logging;
using UserManagement.DTOs;

namespace UserManagement.Services
{
    public class MailService(
        IOptions<MailSettings> mailSettings,
        IFormateService formateService,
        ILogger<MailService> logger)
        : IMailService
    {
        private readonly MailSettings _mailSettings = mailSettings.Value;

        public async Task<ErrorOr<bool>> SendEmailAsync(MailRequestDto mailRequest)
        {
            logger.LogInformation("Start sending message {body} to the user {user-mail}", mailRequest.Body, mailRequest.mailTo);

            if (string.IsNullOrEmpty(mailRequest.mailTo) || !mailRequest.mailTo.Contains('@'))
                return MailErrors.InvalidEmail;

            var email = new MimeMessage
            {
                Sender = MailboxAddress.Parse(_mailSettings.Username),
                Subject = mailRequest.Subject
            };

            email.To.Add(MailboxAddress.Parse(mailRequest.mailTo));

            var builder = new BodyBuilder();

            // Create HTML email template
            var htmlBody = formateService.GenerateHtmlBody(_mailSettings.DisplayName ?? "No Name", mailRequest.Body);
            if (htmlBody.IsError)
            {
                return Error.Failure(description: "can not generate Html Body");
            }

            builder.HtmlBody = htmlBody.Value;

            // Handle attachments
            if (mailRequest.attachments != null && mailRequest.attachments.Any())
            {
                foreach (var file in mailRequest.attachments)
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
    }
}