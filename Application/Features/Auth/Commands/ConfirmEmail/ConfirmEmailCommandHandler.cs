using Application.Interfaces.Services;
using Domain.Constants;
using Domain.Entities;
using Domain.Exceptions.DomainExceptions;
using Domain.Interfaces.Repositories;
using Domain.ValueObjects;
using MediatR;

namespace Application.Features.Auth.Commands.ConfirmEmail
{
    public class ConfirmEmailCommandHandler : IRequestHandler<ConfirmEmailCommand, Unit>
    {
        private readonly ICacheService _cache;
        private readonly IUnitOfWork _uow;

        public ConfirmEmailCommandHandler(ICacheService cache, IUnitOfWork uow)
        {
            _cache = cache;
            _uow = uow;
        }

        public async Task<Unit> Handle(ConfirmEmailCommand request, CancellationToken ct)
        {
            // Validate email verification context
            if (request.WalletAddress != await _cache.GetAsync($"email-verify:{request.Email}"))
                throw new BusinessRuleViolationException(
                    "Invalid email verification context",
                    ErrorCodes.Forbidden);

            // Verify OTP
            if (request.OTP != await _cache.GetAsync($"otp:{request.Email}"))
                throw new BusinessRuleViolationException(
                    "OTP is not correct",
                    ErrorCodes.OTPMismatch);

            // Delete current context and otp in db
            await _cache.DeleteAsync($"otp:{request.Email}");
            await _cache.DeleteAsync($"email-verify:{request.Email}");


            // Create user
            var wallet = WalletAddress.Create(request.WalletAddress);
            var email = Email.Create(request.Email);
            var user = User.Register(wallet, email);
            await _uow.Users.CreateAsync(user);

            return Unit.Value;
        }
    }
}
