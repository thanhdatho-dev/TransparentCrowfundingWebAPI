using Application.DTOs.Auth;
using Application.DTOs.Services.WalletSignature;

namespace Application.Interfaces.Services
{
    public interface IAuthService
    {
        Task<string?> VerifyEmailAsync(EmailVerifyDto dto);
        Task ConfirmEmailAsync(EmailConfirmWithOTP client, string userId);
        Task<LoginDto> LoginAsync(WalletSignatureDto dto);
        Task<string?> GetRefreshTokenAsync(string userId);
        Task<RefreshTokenDto> RefreshTokenAsync(string userId, string accesToken);
        Task RevokeRefreshTokenAsync(string userId);
        Task<bool> IsTokenBlacklistedAsync(string jti);

    }
}
