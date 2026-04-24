using FluentValidation;

namespace Application.Features.Auth.Commands.Refresh
{
    public class RefreshCommandValidator : AbstractValidator<RefreshCommand>
    {
        public RefreshCommandValidator()
        {
            RuleFor(x => x.TokenJti)
                .NotEmpty();

            RuleFor(x => x.UserId)
                .NotEmpty();

            RuleFor(x => x.UsedAccessToken)
                .NotEmpty();
        }
    }
}
