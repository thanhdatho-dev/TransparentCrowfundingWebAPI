using Application.DTOs.Services.MailSender;

namespace Application.Interfaces.Repositories.Services
{
    public interface IMailService
    {
        Task SendEmailAsync(MailRequest mailRequest);
    }
}
