using Application.DTOs.AuthDTOs;
using MediatR;

namespace Application.Features.Auth.Commands.Refresh
{
    public record RefreshCommand(
        string TokenJti,
        string UserId,
        string UsedAccessToken,
        string UsedRefreshToken) : IRequest<TokenDto>;
}
