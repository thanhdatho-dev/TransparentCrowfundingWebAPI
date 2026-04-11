using Domain.DTOs.Services.JWTs;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace Application.Interfaces.Services
{
    public interface ITokenService
    {
        string GenerateAccessToken(User user);
        string GenerateRefreshToken();

        TokenDto? RefreshToken(User user, string clientRefreshToken);

        bool RevokeToken(User user);
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    }
}
