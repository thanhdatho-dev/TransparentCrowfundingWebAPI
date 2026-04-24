using Domain.ValueObjects;
using FluentValidation;

namespace Application.Features.Auth.Queries.CheckWalletRegistered
{
    public class CheckWalletRegisteredQueryValidator
        : AbstractValidator<CheckWalletRegisteredQuery>
    {
        public CheckWalletRegisteredQueryValidator()
        {
            RuleFor(x => x.WalletAddress)
                .NotEmpty()
                .Must(addr => WalletAddress.TryCreate(addr, out _))
                .WithMessage("Invalid Ethereum wallet address");
        }
    }
}
