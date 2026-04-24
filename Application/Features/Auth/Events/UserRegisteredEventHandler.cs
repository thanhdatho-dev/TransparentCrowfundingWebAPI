using Application.Interfaces.Services;
using Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Auth.Events
{
    /// <summary>
    /// Side-effect handler: gửi welcome email khi user đăng ký thành công.
    /// Tách riêng khỏi ConfirmEmailCommandHandler — tuân thủ Single Responsibility.
    /// </summary>
    public class UserRegisteredEventHandler : INotificationHandler<UserRegisteredEvent>
    {
        private readonly IEmailService _emailService;
        private readonly ILogger<UserRegisteredEventHandler> _logger;

        public UserRegisteredEventHandler(
            IEmailService emailService,
            ILogger<UserRegisteredEventHandler> logger)
        {
            _emailService = emailService;
            _logger = logger;
        }

        public async Task Handle(UserRegisteredEvent notification, CancellationToken ct)
        {
            _logger.LogInformation(
                "User registered: {UserId}, Email: {Email}, Wallet: {Wallet}",
                notification.UserId,
                notification.Email,
                notification.WalletAddress);

            try
            {
                await _emailService.SendAsync(
                    notification.Email,
                    "Welcome to Transparent Crowdfunding!",
                    BuildWelcomeEmail(notification.WalletAddress),
                    ct);
            }
            catch (Exception ex)
            {
                // Log nhưng KHÔNG throw — side-effect không nên fail main flow
                _logger.LogError(ex, "Failed to send welcome email to {Email}", notification.Email);
            }
        }

        private static string BuildWelcomeEmail(string walletAddress) =>
            $"""
            <h2>Chào mừng bạn đến với Transparent Crowdfunding!</h2>
            <p>Tài khoản của bạn đã được tạo thành công.</p>
            <p>Ví liên kết: <strong>{walletAddress}</strong></p>
            <hr/>
            <p>Cảm ơn bạn đã tham gia nền tảng của chúng tôi.</p>
            """;
    }
}
