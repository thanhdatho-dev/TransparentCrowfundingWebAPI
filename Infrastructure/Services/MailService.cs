using Application.DTOs.Services.MailSender;
using Application.Interfaces.Repositories.Services;
using Domain.Configurations;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Infrastructure.Services
{
    public class MailService(IOptions<MailSettings> mailSettings) : IMailService
    {
        private readonly MailSettings _mailSettings = mailSettings.Value;
        public async Task SendEmailAsync(MailRequest mailRequest)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(_mailSettings.DisplayName, _mailSettings.Mail!));
            email.To.Add(MailboxAddress.Parse(mailRequest.ToEmail!));
            email.Subject = mailRequest.Subject!;
            var builder = new BodyBuilder
            {
                HtmlBody = mailRequest.Body
            };
            email.Body = builder.ToMessageBody();
            using var smtp = new SmtpClient();
            smtp.Connect(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);
            smtp.Authenticate(_mailSettings.Username, _mailSettings.Password);
            await smtp.SendAsync(email);
            smtp.Disconnect(true);
        }
    }
}
