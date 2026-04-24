using Application.DTOs.AuthDTOs;
using Application.Interfaces.Services;
using Domain.Constants;
using Domain.Exceptions.DomainExceptions;
using Domain.Interfaces.Repositories;
using Domain.ValueObjects;
using MediatR;

namespace Application.Features.Auth.Commands.Login
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginDto>
    {
        private readonly ICacheService _cache;
        private readonly IWalletSignatureVerifier _walletVerifier;
        private readonly IUnitOfWork _uow;
        private readonly ITokenService _tokenService;

        public LoginCommandHandler(
            ICacheService cache,
            IWalletSignatureVerifier walletVerifier,
            IUnitOfWork uow,
            ITokenService tokenService)
        {
            _cache = cache;
            _walletVerifier = walletVerifier;
            _uow = uow;
            _tokenService = tokenService;
        }

        public async Task<LoginDto> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            // 1. Verify Wallet
            var walletAddress = WalletAddress.Create(request.WalletAddress).ToString();
            if (!_walletVerifier.VerifySignature(request.Message, request.Signature, walletAddress))
                throw new BusinessRuleViolationException(
                    "Invalid wallet signature",
                    ErrorCodes.InvalidWallet);

            // 2. Find user by username
            var user = await _uow.Users.FindByNameAsync(walletAddress)
                ?? throw new EntityNotFoundException(
                    "Account not found, please verify email first",
                    ErrorCodes.NotFound);

            // 3. Generate tokens
            var accessToken = await _tokenService.GenerateAccessToken(user).ConfigureAwait(false);
            var refreshToken = _tokenService.GenerateRefreshToken();
            var expiry = TimeSpan.FromDays(AuthConstants.RefreshTokenExpiryDays);
            var expiryTime = DateTime.UtcNow.Add(expiry);

            await _cache.DeleteAsync($"refresh:{user.Id}");
            await _cache.SetAsync($"refresh:{user.Id}", refreshToken, expiry);

            return new LoginDto
            {
                UserId = user.Id,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiryRefreshTokenTime = expiryTime,
            };
        }
    }
}
