using Application.Features.Auth.Commands.Logout;
using Application.Interfaces.Services;
using FluentAssertions;
using MediatR;
using Moq;

namespace Application.Tests.Features.Auth.Commands.Logout
{
    public class LogoutCommandHandlerTests
    {
        private readonly Mock<ICacheService> _cacheMock = new();
        private readonly Mock<ITokenService> _tokenServiceMock = new();
        private readonly LogoutCommandHandler _sut;

        private const string UserId = "user-id-123";
        private const string Jti = "jti-123";
        private const string UsedAccessToken = "fake-access-token";

        public LogoutCommandHandlerTests()
        {
            _sut = new LogoutCommandHandler(_cacheMock.Object, _tokenServiceMock.Object);
        }

        [Fact]
        public async Task Handle_ValidRequest_BlacklistsAccessTokenAndDeletesRefreshToken()
        {
            // Arrange
            var remainingTtl = TimeSpan.FromMinutes(10);
            _tokenServiceMock.Setup(x => x.GetRemainingTtl(UsedAccessToken))
                .Returns(remainingTtl);

            var command = new LogoutCommand(UserId, Jti, UsedAccessToken);

            // Act
            var result = await _sut.Handle(command, CancellationToken.None);

            // Assert
            result.Should().Be(Unit.Value);
            _cacheMock.Verify(x => x.SetAsync(
                $"blacklist:jti:{Jti}",
                "revoke",
                remainingTtl), Times.Once);
            _cacheMock.Verify(x => x.DeleteAsync($"refresh:{UserId}"), Times.Once);
        }

        [Fact]
        public async Task Handle_ExpiredAccessToken_SkipsBlacklistButDeletesRefreshToken()
        {
            // Arrange
            _tokenServiceMock.Setup(x => x.GetRemainingTtl(UsedAccessToken))
                .Returns(TimeSpan.Zero);

            var command = new LogoutCommand(UserId, Jti, UsedAccessToken);

            // Act
            var result = await _sut.Handle(command, CancellationToken.None);

            // Assert
            result.Should().Be(Unit.Value);
            _cacheMock.Verify(x => x.SetAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<TimeSpan>()), Times.Never);
            _cacheMock.Verify(x => x.DeleteAsync($"refresh:{UserId}"), Times.Once);
        }
    }
}
