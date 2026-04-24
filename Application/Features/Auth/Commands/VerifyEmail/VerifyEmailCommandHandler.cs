using Application.Interfaces.Services;
using Domain.Constants;
using Domain.Exceptions.DomainExceptions;
using Domain.Interfaces.Repositories;
using Domain.ValueObjects;
using MediatR;

namespace Application.Features.Auth.Commands.VerifyEmail
{
    public class VerifyEmailCommandHandler : IRequestHandler<VerifyEmailCommand, Unit>
    {
        private readonly ICacheService _cache;
        private readonly IEmailService _emailService;
        private readonly IWalletSignatureVerifier _walletVerifier;
        private readonly IUnitOfWork _uow;

        public VerifyEmailCommandHandler(
            IUnitOfWork uow,
            IWalletSignatureVerifier walletVerifier,
            ICacheService cache,
            IEmailService emailService)
        {
            _cache = cache;
            _emailService = emailService;
            _walletVerifier = walletVerifier;
            _uow = uow;
        }

        public async Task<Unit> Handle(VerifyEmailCommand request, CancellationToken ct)
        {
            // 1. Verify wallet signature
            var walletAddress = WalletAddress.Create(request.WalletAddress).ToString();
            var email = Email.Create(request.Email).ToString();
            if (!_walletVerifier.VerifySignature(request.Message, request.Signature, walletAddress))
                throw new BusinessRuleViolationException(
                    "Invalid wallet signature",
                    ErrorCodes.InvalidWallet);

            // 2. Check email exists
            if (await _uow.Users.ExistsByEmailAsync(email, ct))
                throw new ExistedEntityException(
                    "Email exists",
                    ErrorCodes.EmailAlreadyUsed);

            // 3. Generate OTP
            string OTP = GenerateOTP();

            // 4. Cache OTP and Wallet context
            var expiry = TimeSpan.FromMinutes(AuthConstants.OTPExpiryMinutes);
            await _cache.SetAsync($"otp:{email}", OTP, expiry);
            await _cache.SetAsync($"email-verify:{email}", walletAddress, expiry);

            // 5. Send Email

            _ = Task.Run(() => _emailService.SendAsync(
                to: email,
                subject: "Email Verification OTP",
                body: BuildEmailBody(OTP),
                ct), ct);

            return Unit.Value;
        }

        private static string GenerateOTP()
        {
            var chars = "0123456789".ToCharArray();
            return string.Join("", Random.Shared.GetItems(chars, AuthConstants.OTPLength));
        }

        private static string BuildEmailBody(string otp) =>
        $"""
        <p>Nhập mã OTP sau để xác thực email:</p>
        <hr/>
        <h2>{otp}</h2>
        <hr/>
        <p>Mã sẽ hết hạn sau {AuthConstants.OTPExpiryMinutes} phút</p>
        """;
    }
}
