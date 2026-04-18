using Application.Interfaces.Repositories;
using Microsoft.AspNetCore.Http;
using StackExchange.Redis;
using System.IdentityModel.Tokens.Jwt;

namespace Infrastructure.Repositories
{
    public class TokenRepository(IConnectionMultiplexer redis, IHttpContextAccessor httpContext) : ITokenRepository
    {
        private readonly IDatabase _db = redis.GetDatabase();
        private readonly IHttpContextAccessor _httpContext = httpContext;

        public async Task<string?> GetRefreshTokenAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new Exception("User not found");
            return await _db.StringGetAsync($"refresh:{userId}");
        }

        public async Task SaveRefreshTokenAsync(string userId, string refreshToken, TimeSpan expiry)
        {
            await _db.StringSetAsync($"refresh:{userId}", refreshToken, expiry);
        }

        public async Task DeleteRefreshTokenAsync(string userId)
        {
            await _db.KeyDeleteAsync($"refresh:{userId}");
            if (_httpContext.HttpContext is null)
                throw new InvalidOperationException("HttpContext is not available");
            _httpContext.HttpContext.Response.Cookies.Delete("refreshToken");
        }

        public async Task BlacklistAccessTokenAsync(string accessToken)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(accessToken);

            var jti = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;
            if (string.IsNullOrEmpty(jti)) return;

            var expClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Exp)?.Value;
            if (string.IsNullOrEmpty(expClaim)) return;

            var exp = DateTimeOffset.FromUnixTimeSeconds(long.Parse(expClaim));
            var ttl = exp - DateTime.UtcNow;

            if (ttl > TimeSpan.Zero)
            {
                await _db.StringSetAsync($"blacklist:jti:{jti}", "revoked", ttl);
            }
        }

        public async Task<bool> IsTokenBlacklistedAsync(string jti)
        {
            if (string.IsNullOrEmpty(jti))
                throw new ArgumentException("JTI cannot be null or empty", nameof(jti));
            return await _db.KeyExistsAsync($"blacklist:jti:{jti}");
        }
    }
}
