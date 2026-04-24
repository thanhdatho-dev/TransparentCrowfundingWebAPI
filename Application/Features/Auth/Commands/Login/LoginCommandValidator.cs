using FluentValidation;

namespace Application.Features.Auth.Commands.Login
{
    public class LoginCommandValidator : AbstractValidator<LoginCommand>
    {
        public LoginCommandValidator()
        {
            RuleFor(x => x.Message)
                .NotEmpty();

            RuleFor(x => x.Signature)
                .NotEmpty();
        }
    }
}
