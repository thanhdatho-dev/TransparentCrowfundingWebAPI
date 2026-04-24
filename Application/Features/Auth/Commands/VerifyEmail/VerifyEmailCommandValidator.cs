using FluentValidation;

namespace Application.Features.Auth.Commands.VerifyEmail
{
    public class VerifyEmailCommandValidator : AbstractValidator<VerifyEmailCommand>
    {
        public VerifyEmailCommandValidator()
        {
            RuleFor(x => x.Message)
                .NotEmpty();

            RuleFor(x => x.Signature)
                .NotEmpty();
        }
    }
}
