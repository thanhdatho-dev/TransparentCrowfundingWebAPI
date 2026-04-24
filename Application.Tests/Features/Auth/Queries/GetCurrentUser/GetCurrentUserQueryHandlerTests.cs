using Application.Features.Auth.Queries.GetCurrentUser;
using Domain.Constants;
using Domain.Entities;
using Domain.Exceptions.DomainExceptions;
using Domain.Interfaces.Repositories;
using Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace Application.Tests.Features.Auth.Queries.GetCurrentUser
{
    public class GetCurrentUserQueryHandlerTests
    {
        private readonly Mock<IUnitOfWork> _uowMock = new();
        private readonly GetCurrentUserQueryHandler _sut;

        public GetCurrentUserQueryHandlerTests()
        {
            _sut = new GetCurrentUserQueryHandler(_uowMock.Object);
        }

        [Fact]
        public async Task Handle_UserExists_ReturnsUserProfileDto()
        {
            // Arrange
            var wallet = WalletAddress.Create("0x1234567890abcdef1234567890abcdef12345678");
            var email = Email.Create("test@example.com");
            var user = User.Register(wallet, email);

            _uowMock.Setup(x => x.Users.FindByIdAsync(user.Id.ToString()))
                .ReturnsAsync(user);

            var query = new GetCurrentUserQuery(user.Id.ToString());

            // Act
            var result = await _sut.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(user.Id);
            result.WalletAddress.Should().Be(wallet.Value);
            result.Email.Should().Be(email.Value);
            result.EmailConfirmed.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_UserNotFound_ThrowsEntityNotFoundException()
        {
            // Arrange
            _uowMock.Setup(x => x.Users.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync((User?)null);

            var query = new GetCurrentUserQuery("nonexistent-id");

            // Act
            var act = () => _sut.Handle(query, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<EntityNotFoundException>()
                .Where(e => e.ErrorCode == ErrorCodes.NotFound);
        }
    }
}
