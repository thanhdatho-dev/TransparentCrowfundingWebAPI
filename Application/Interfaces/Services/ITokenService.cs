using Application.DTOs.AuthDTOs;
using Domain.Entities;
using System.Security.Claims;

namespace Application.Interfaces.Services
{
    public interface ITokenService
    {
        string GenerateAccessToken(User user);
        string GenerateRefreshToken();
        TimeSpan GetRemainingTtl(string accessToken);
    }
}
