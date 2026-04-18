using Application.DTOs.Services.JWTs;
using Domain.Entities;
using System.Security.Claims;

namespace Application.Interfaces.Repositories.Services
{
    public interface ITokenService
    {
        string GenerateAccessToken(User user);
        string GenerateRefreshToken();
        TokenDto RefreshToken(User user);
        Task SetRefreshTokenCookie(string refreshToken, DateTime? expires);
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    }
}
