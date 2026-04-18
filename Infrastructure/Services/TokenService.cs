using Application.DTOs.Services.JWTs;
using Application.Interfaces.Repositories.Services;
using Domain.Configurations;
using Domain.Constants;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Services
{
    public class TokenService(IOptions<JWT> jwtSettings, IHttpContextAccessor httpContext) : ITokenService
    {
        private readonly JWT _jwtSettings = jwtSettings.Value;
        private readonly SymmetricSecurityKey _key = new(Encoding.UTF8.GetBytes(s: jwtSettings.Value.SigningKey));
        private readonly IHttpContextAccessor _httpContext = httpContext;

        public string GenerateAccessToken(User user)
        {
            var jti = Guid.NewGuid().ToString();
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Email, user.Email!),
                new(JwtRegisteredClaimNames.EmailVerified, user.EmailConfirmed.ToString()),
                new(JwtRegisteredClaimNames.NameId, user.Id),
                new(JwtRegisteredClaimNames.Jti, jti)
            };

            var cred = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256Signature);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(AuthConstansts.OTPExpiryMinutes),
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
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        public TokenDto RefreshToken(User user)
            => new()
            {
                AccessToken = GenerateAccessToken(user),
                RefreshToken = GenerateRefreshToken()
            };

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

        public async Task SetRefreshTokenCookie(string refreshToken, DateTime? expires)
        {
            try
            {
                if (string.IsNullOrEmpty(refreshToken))
                    throw new ArgumentException("Refresh token cannot be empty");

                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Expires = expires,
                    IsEssential = true,
                    Path = "/"
                };
                _httpContext.HttpContext.Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
            }
            catch (Exception ex)
            {
                throw new(ex.Message);
            }
        }
    }
}
