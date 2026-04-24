using MediatR;

namespace Application.Features.Auth.Queries.CheckWalletRegistered
{
    public record CheckWalletRegisteredQuery(string WalletAddress) : IRequest<bool>;
}
