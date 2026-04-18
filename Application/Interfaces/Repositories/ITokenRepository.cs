namespace Application.Interfaces.Repositories
{
    public interface ITokenRepository
    {
        Task<string?> GetRefreshTokenAsync(string userId);
        Task SaveRefreshTokenAsync(string userId, string refreshToken, TimeSpan expiry);
        Task DeleteRefreshTokenAsync(string userId);
        Task BlacklistAccessTokenAsync(string accessToken);
        Task<bool> IsTokenBlacklistedAsync(string jti);
    }
}
