using Application.Interfaces.Services;
using Domain.Constants;
using Domain.Entities;
using Infrastructure.Identity;
using Infrastructure.Mappers;
using Infrastructure.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Services
{
    public class TokenService(IOptions<JWT> jwtSettings, UserManager<AppUser> userManager, ICacheService cacheService) : ITokenService
    {
        private readonly JWT _jwtSettings = jwtSettings.Value;
        private readonly UserManager<AppUser> _userManager = userManager;
        private readonly ICacheService _cacheService = cacheService;
        private readonly SymmetricSecurityKey _key = new(Encoding.UTF8.GetBytes(s: jwtSettings.Value.SigningKey));

        public async Task<string> GenerateAccessToken(User user)
        {
            var jti = Guid.NewGuid().ToString();

            string cacheKey = $"roles:{user.Id}";
            string? cachedRoles = await _cacheService.GetAsync(cacheKey);
            IList<string> roles;

            if (!string.IsNullOrEmpty(cachedRoles))
            {
                roles = cachedRoles.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
            }
            else
            {
                roles = await _userManager.GetRolesAsync(user.FromUserToAppUserForRead());
                if (roles.Any())
                {
                    await _cacheService.SetAsync(cacheKey, string.Join(",", roles), TimeSpan.FromHours(1));
                }
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.Email, user.Email.ToString()!),
                new(JwtRegisteredClaimNames.EmailVerified, user.EmailConfirmed.ToString()),
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(JwtRegisteredClaimNames.Jti, jti)
            };

            foreach(var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var cred = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256Signature);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(AuthConstants.OTPExpiryMinutes),
                SigningCredentials = cred,
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        public TimeSpan GetRemainingTtl(string accessToken)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(accessToken);
            var remaining = jwtToken.ValidTo - DateTime.UtcNow;
            return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
        }
    }
}
