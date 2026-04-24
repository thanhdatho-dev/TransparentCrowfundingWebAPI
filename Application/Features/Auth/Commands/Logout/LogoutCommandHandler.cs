using Application.Interfaces.Services;
using MediatR;

namespace Application.Features.Auth.Commands.Logout
{
    public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Unit>
    {
        private readonly ICacheService _cache;
        private readonly ITokenService _tokenService;

        public LogoutCommandHandler(ICacheService cache, ITokenService tokenService)
        {
            _cache = cache;
            _tokenService = tokenService;
        }

        public async Task<Unit> Handle(LogoutCommand request, CancellationToken ct = default)
        {
            // Blacklist the access token for its remaining TTL
            var remainingTtl = _tokenService.GetRemainingTtl(request.UsedAccessToken);
            if (remainingTtl > TimeSpan.Zero)
                await _cache.SetAsync(
                    $"blacklist:jti:{request.Jti}",
                    "revoke",
                    remainingTtl);

            await _cache.DeleteAsync($"refresh:{request.UserId}");
            return Unit.Value;
        }
    }
}
