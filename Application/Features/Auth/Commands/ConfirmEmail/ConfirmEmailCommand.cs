using MediatR;

namespace Application.Features.Auth.Commands.ConfirmEmail
{
    public record ConfirmEmailCommand(string Email, string WalletAddress, string OTP) : IRequest<Unit>;
}
