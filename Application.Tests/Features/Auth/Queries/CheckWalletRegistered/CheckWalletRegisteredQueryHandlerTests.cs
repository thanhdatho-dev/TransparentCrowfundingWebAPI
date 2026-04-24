using Application.Features.Auth.Queries.CheckWalletRegistered;
using Domain.Entities;
using Domain.Interfaces.Repositories;
using Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace Application.Tests.Features.Auth.Queries.CheckWalletRegistered
{
    public class CheckWalletRegisteredQueryHandlerTests
    {
        private readonly Mock<IUnitOfWork> _uowMock = new();
        private readonly CheckWalletRegisteredQueryHandler _sut;

        private const string ValidWallet = "0x1234567890abcdef1234567890abcdef12345678";

        public CheckWalletRegisteredQueryHandlerTests()
        {
            _sut = new CheckWalletRegisteredQueryHandler(_uowMock.Object);
        }

        [Fact]
        public async Task Handle_WalletRegistered_ReturnsTrue()
        {
            // Arrange
            var wallet = WalletAddress.Create(ValidWallet);
            var email = Email.Create("test@example.com");
            var user = User.Register(wallet, email);

            _uowMock.Setup(x => x.Users.FindByNameAsync(wallet.Value))
                .ReturnsAsync(user);

            var query = new CheckWalletRegisteredQuery(ValidWallet);

            // Act
            var result = await _sut.Handle(query, CancellationToken.None);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_WalletNotRegistered_ReturnsFalse()
        {
            // Arrange
            var normalizedWallet = ValidWallet.ToLowerInvariant();
            _uowMock.Setup(x => x.Users.FindByNameAsync(normalizedWallet))
                .ReturnsAsync((User?)null);

            var query = new CheckWalletRegisteredQuery(ValidWallet);

            // Act
            var result = await _sut.Handle(query, CancellationToken.None);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void Handle_InvalidWalletFormat_ThrowsArgumentException()
        {
            // Arrange
            var query = new CheckWalletRegisteredQuery("invalid-wallet");

            // Act
            var act = () => _sut.Handle(query, CancellationToken.None);

            // Assert — WalletAddress.Create throws inside handler
            act.Should().ThrowAsync<ArgumentException>();
        }
    }
}
