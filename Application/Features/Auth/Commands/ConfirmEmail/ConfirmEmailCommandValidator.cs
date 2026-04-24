using Domain.Constants;
using FluentValidation;

namespace Application.Features.Auth.Commands.ConfirmEmail
{
    public class ConfirmEmailCommandValidator : AbstractValidator<ConfirmEmailCommand>
    {
        public ConfirmEmailCommandValidator()
        {
            RuleFor(x => x.OTP)
                .NotEmpty()
                .Length(exactLength: AuthConstants.OTPLength);
        }
    }
}
