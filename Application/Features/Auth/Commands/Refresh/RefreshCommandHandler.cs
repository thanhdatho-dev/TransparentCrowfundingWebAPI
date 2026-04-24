using Application.DTOs.AuthDTOs;
using Application.Exceptions;
using Application.Interfaces.Services;
using Domain.Constants;
using Domain.Interfaces.Repositories;
using MediatR;


namespace Application.Features.Auth.Commands.Refresh
{
    public class RefreshCommandHandler : IRequestHandler<RefreshCommand, TokenDto>
    {
        private readonly ICacheService _cache;
        private readonly ITokenService _tokenService;
        private readonly IUnitOfWork _uow;

        public RefreshCommandHandler(ICacheService cache, ITokenService tokenService, IUnitOfWork uow)
        {
            _cache = cache;
            _tokenService = tokenService;
            _uow = uow;
        }

        public async Task<TokenDto> Handle(RefreshCommand request, CancellationToken cancellationToken)
        {
            // Throw if in used access token is blacklisted
            if (await _cache.ExistsAsync($"blacklist:jti:{request.TokenJti}"))
                throw new UnauthorizedException(
                    "Action is unauthorized",
                    ErrorCodes.Unauthorized);

            // Verify refresh token
            var refreshToken = await _cache.GetAsync($"refresh:{request.UserId}");
            if (string.IsNullOrEmpty(refreshToken) || request.UsedRefreshToken != refreshToken)
                throw new UnauthorizedException(
                    "Action is unauthorized",
                    ErrorCodes.Unauthorized);

            // Verify user
            var user = await _uow.Users.FindByIdAsync(request.UserId)
                ?? throw new UnauthorizedException(
                    "Action is unauthorized",
                    ErrorCodes.Unauthorized);

            // Refresh token
            var expiry = TimeSpan.FromDays(AuthConstants.RefreshTokenExpiryDays);
            var expiryTime = DateTime.UtcNow.Add(expiry);
            var newAccessToken = _tokenService.GenerateAccessToken(user);
            var newRefreshToken = _tokenService.GenerateRefreshToken();

            var remainingTtl = _tokenService.GetRemainingTtl(request.UsedAccessToken);
            if (remainingTtl > TimeSpan.Zero)
                await _cache.SetAsync(
                    $"blacklist:jti:{request.TokenJti}",
                    "revoke",
                    remainingTtl);

            await _cache.DeleteAsync($"refresh:{request.UserId}");
            await _cache.SetAsync($"refresh:{request.UserId}", newRefreshToken, expiry);


            return new TokenDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                RefreshTokenExiryTime = expiryTime
            };
        }
    }
}
