using Application.DTOs.AuthDTOs;
using MediatR;

namespace Application.Features.Auth.Commands.Login
{
    public record LoginCommand(
        string WalletAddress,
        string Message,
        string Signature
    ) : IRequest<LoginDto>;
}
