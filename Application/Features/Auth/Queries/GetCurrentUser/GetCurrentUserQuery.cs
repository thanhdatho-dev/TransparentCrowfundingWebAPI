using Application.DTOs.AuthDTOs;
using MediatR;

namespace Application.Features.Auth.Queries.GetCurrentUser
{
    public record GetCurrentUserQuery(string UserId) : IRequest<UserProfileDto>;
}
