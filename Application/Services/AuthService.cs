using Application.DTOs.Auth;
using Application.DTOs.Services.MailSender;
using Application.DTOs.Services.WalletSignature;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Services;
using Application.Interfaces.Services;
using Domain.Constants;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public class AuthService(
        UserManager<User> userManager,
        IWalletSignatureVerifier signatureVerifier,
        ITokenRepository tokenRepo,
        ITokenService tokenService,
        IMailRepository mailRepo,
        IMailService mailService) : IAuthService
    {
        private readonly UserManager<User> _userManager = userManager;
        private readonly IWalletSignatureVerifier _signatureVerifier = signatureVerifier;
        private readonly ITokenRepository _tokenRepo = tokenRepo;
        private readonly ITokenService _tokenService = tokenService;
        private readonly IMailRepository _mailRepo = mailRepo;
        private readonly IMailService _mailService = mailService;

        public async Task<string?> VerifyEmailAsync(EmailVerifyDto emailVerifyDto)
        {
            try
            {
                var ws = emailVerifyDto.WalletSignature;
                if (!_signatureVerifier.VerifySignature
                (
                    ws!.Message,
                    ws.Signature,
                    ws.Address
                ))
                    throw new("Cannot verify wallet");

                var user = await _userManager.FindByNameAsync(ws.Address);

                if (user != null && user.EmailConfirmed)
                    throw new("Email already used");

                if (user == null)
                {
                    await _userManager.CreateAsync(user = new User
                    {
                        UserName = ws.Address,
                        Email = emailVerifyDto.Email,
                    });
                }

                string emailVerifyOTP = _mailRepo.GenerateSecureOTP();

                var mailRequest = new MailRequest
                {
                    ToEmail = emailVerifyDto.Email,
                    Subject = "Verify Email OTP",
                    Body = "<p>Nhập mã OTP sau để xác thực email:</p>" +
                        "</hr>" +
                        $"<p>{emailVerifyOTP}</p>" +
                        $"</hr>" +
                        $"<p>Mã sẽ hết hạn sau {AuthConstansts.OTPExpiryMinutes} phút</p>"
                };

                await _mailRepo.SaveOTPAsync(user.Id, emailVerifyOTP);
                await _mailService.SendEmailAsync(mailRequest);
                await _userManager.UpdateAsync(user);

                return user.Id;
            }
            catch (Exception ex)
            {
                throw new(ex.Message);
            }
        }

        public async Task<LoginDto> LoginAsync(WalletSignatureDto dto)
        {
            if (!_signatureVerifier.VerifySignature(
            dto.Message,
            dto.Signature,
            dto.Address))
                throw new InvalidOperationException("Wallet verification failed");

            var user = await _userManager.Users
                .FirstOrDefaultAsync(x => x.UserName == dto.Address) ?? throw new InvalidOperationException("User not found");
            if (!user.EmailConfirmed)
                throw new InvalidOperationException("Email verification needed");

            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();
            var expiryRefreshTokenTime = DateTime.UtcNow.AddDays(AuthConstansts.RefreshTokenExpiryDays);

            await _tokenRepo.SaveRefreshTokenAsync(user.Id, refreshToken, TimeSpan.FromDays(AuthConstansts.RefreshTokenExpiryDays));
            await _tokenService.SetRefreshTokenCookie(refreshToken, expiryRefreshTokenTime);
            await _userManager.UpdateAsync(user);

            return new LoginDto
            {
                UserId = user.Id,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiryRefreshTokenTime = expiryRefreshTokenTime
            };
        }

        public async Task RevokeRefreshTokenAsync(string userId)
        {
            await _tokenRepo.DeleteRefreshTokenAsync(userId);
        }

        public async Task ConfirmEmailAsync(EmailConfirmWithOTP client, string userId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new("UserId is invalid");

            string? otp = await _mailRepo.GetOTPAsync(userId) ?? throw new("Invalid OTP");

            if (otp != client.ClientOTP)
                throw new InvalidOperationException("OTP does not match");

            var user = await _userManager.FindByIdAsync(userId) ?? throw new("Invalid user's request");

            user.EmailConfirmed = true;

            await _userManager.UpdateAsync(user);
            await _mailRepo.DeleteOTPAsync(userId);
        }

        public async Task<bool> IsTokenBlacklistedAsync(string jti)
        {
            return await _tokenRepo.IsTokenBlacklistedAsync(jti);
        }

        public async Task<string?> GetRefreshTokenAsync(string userId)
        {
            return await _tokenRepo.GetRefreshTokenAsync(userId);
        }

        public async Task<RefreshTokenDto> RefreshTokenAsync(string userId, string accessToken)
        {
            try
            {
                var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == userId) ?? throw new("Invalid user");
                var tokens = _tokenService.RefreshToken(user);
                if (string.IsNullOrEmpty(tokens.AccessToken) || string.IsNullOrEmpty(tokens.RefreshToken))
                    throw new("Cannot generate new tokens");

                var expiryRefreshTokenTime = DateTime.UtcNow.AddDays(AuthConstansts.RefreshTokenExpiryDays);

                await _tokenRepo.DeleteRefreshTokenAsync(userId);
                await _tokenRepo.SaveRefreshTokenAsync(userId, tokens.RefreshToken, TimeSpan.FromDays(AuthConstansts.RefreshTokenExpiryDays));
                await _tokenService.SetRefreshTokenCookie(tokens.RefreshToken, expiryRefreshTokenTime);
                await _tokenRepo.BlacklistAccessTokenAsync(accessToken);

                return new RefreshTokenDto
                {
                    AccessToken = tokens.AccessToken,
                    RefreshToken = tokens.RefreshToken,
                    ExpiryRefreshTokenTime = expiryRefreshTokenTime
                };
            }
            catch (Exception ex)
            {
                throw new(ex.Message);
            }
        }
    }
}
