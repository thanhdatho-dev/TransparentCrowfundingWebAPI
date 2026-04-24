using Domain.ValueObjects;
using FluentValidation;

namespace Application.Validators
{
    public class WalletAddressValidator : AbstractValidator<WalletAddress>
    {
        public WalletAddressValidator()
        {
            RuleFor(x => x.Value)
                .NotEmpty()
                .Must(addr => WalletAddress.TryCreate(addr, out _))
                .WithMessage("Invalid Ethereum wallet address");
        }
    }
}
