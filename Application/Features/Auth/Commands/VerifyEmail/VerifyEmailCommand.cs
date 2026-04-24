using MediatR;

namespace Application.Features.Auth.Commands.VerifyEmail
{
    public record VerifyEmailCommand(string Email, string WalletAddress, string Message, string Signature)
        : IRequest<Unit>;
}
