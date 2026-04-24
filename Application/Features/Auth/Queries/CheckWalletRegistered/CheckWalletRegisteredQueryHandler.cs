using Domain.Interfaces.Repositories;
using Domain.ValueObjects;
using MediatR;

namespace Application.Features.Auth.Queries.CheckWalletRegistered
{
    public class CheckWalletRegisteredQueryHandler
        : IRequestHandler<CheckWalletRegisteredQuery, bool>
    {
        private readonly IUnitOfWork _uow;

        public CheckWalletRegisteredQueryHandler(IUnitOfWork uow)
            => _uow = uow;

        public async Task<bool> Handle(
            CheckWalletRegisteredQuery request,
            CancellationToken ct)
        {
            var wallet = WalletAddress.Create(request.WalletAddress);
            var user = await _uow.Users.FindByNameAsync(wallet.Value);
            return user is not null;
        }
    }
}
