using Application.Interfaces.Services;
using Domain.Configurations;
using Domain.DTOs.Services.JWTs;
using Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Repositories.Services
{
    public class TokenService(IOptions<JWT> jwtSettings) : ITokenService
    {
        private readonly JWT _jwtSettings = jwtSettings.Value;
        private readonly SymmetricSecurityKey _key = new(Encoding.UTF8.GetBytes(s: jwtSettings.Value.SigningKey));

        public string GenerateAccessToken(User user)
        {
            return _GenerateAccessToken(user);
        }

        public string GenerateRefreshToken()
        {
            return _GenerateRefreshToken();
        }

        private string _GenerateAccessToken(User user)
        {
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.PreferredUsername, user.UserName!),
                new(JwtRegisteredClaimNames.Email, user.Email!),
                new(JwtRegisteredClaimNames.EmailVerified, user.EmailConfirmed.ToString())
            };

            var cred = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256Signature);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(5),
                SigningCredentials = cred,
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private string _GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        public TokenDto? RefreshToken(User user, string clientRefreshToken)
        {
            if (user.RefreshToken != clientRefreshToken || user.RefreshTokenExpiryTime < DateTime.UtcNow)
                return null;

            var accessToken = GenerateAccessToken(user);
            var refreshToken = GenerateRefreshToken();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            return new TokenDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }

        public bool RevokeToken(User user)
        {
            if (user.RefreshTokenExpiryTime < DateTime.UtcNow)
                return false;

            user.RefreshTokenExpiryTime = DateTime.UtcNow;
            return true;
        }

        public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = _key,
                ValidateLifetime = false // bỏ kiểm tra hạn token
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
                if (securityToken is not JwtSecurityToken jwt ||
                    !jwt.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                    return null;

                return principal;
            }
            catch
            {
                return null;
            }
        }
    }
}
