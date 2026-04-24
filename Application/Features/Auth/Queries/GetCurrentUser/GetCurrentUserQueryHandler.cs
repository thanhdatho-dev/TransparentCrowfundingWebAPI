using Application.DTOs.AuthDTOs;
using Domain.Constants;
using Domain.Exceptions.DomainExceptions;
using Domain.Interfaces.Repositories;
using MediatR;

namespace Application.Features.Auth.Queries.GetCurrentUser
{
    public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, UserProfileDto>
    {
        private readonly IUnitOfWork _uow;

        public GetCurrentUserQueryHandler(IUnitOfWork uow)
            => _uow = uow;

        public async Task<UserProfileDto> Handle(
            GetCurrentUserQuery request,
            CancellationToken ct)
        {
            var user = await _uow.Users.FindByIdAsync(request.UserId)
                ?? throw new EntityNotFoundException(
                    "User not found",
                    ErrorCodes.NotFound);

            return new UserProfileDto
            {
                Id = user.Id,
                WalletAddress = user.WalletAddress.Value,
                Email = user.Email.Value,
                EmailConfirmed = user.EmailConfirmed,
                CreatedAt = user.Created
            };
        }
    }
}
