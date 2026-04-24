using Application.DTOs.AuthDTOs;
using Application.Exceptions;
using Application.Features.Auth.Commands.Refresh;
using Application.Interfaces.Services;
using Domain.Constants;
using Domain.Entities;
using Domain.Interfaces.Repositories;
using Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace Application.Tests.Features.Auth.Commands.Refresh
{
    public class RefreshCommandHandlerTests
    {
        private readonly Mock<ICacheService> _cacheMock = new();
        private readonly Mock<ITokenService> _tokenServiceMock = new();
        private readonly Mock<IUnitOfWork> _uowMock = new();
        private readonly RefreshCommandHandler _sut;

        private const string TokenJti = "jti-123";
        private const string UserId = "user-id-123";
        private const string UsedRefreshToken = "old-refresh-token";
        private const string UsedAccessToken = "old-access-token";

        public RefreshCommandHandlerTests()
        {
            _sut = new RefreshCommandHandler(
                _cacheMock.Object,
                _tokenServiceMock.Object,
                _uowMock.Object);
        }

        private User CreateTestUser()
        {
            var wallet = WalletAddress.Create("0x1234567890abcdef1234567890abcdef12345678");
            var email = Email.Create("test@example.com");
            return User.Register(wallet, email);
        }

        [Fact]
        public async Task Handle_ValidRequest_ReturnsNewTokenDto()
        {
            // Arrange
            var user = CreateTestUser();

            _cacheMock.Setup(x => x.ExistsAsync($"blacklist:jti:{TokenJti}"))
                .ReturnsAsync(false);
            _cacheMock.Setup(x => x.GetAsync($"refresh:{UserId}"))
                .ReturnsAsync(UsedRefreshToken);
            _uowMock.Setup(x => x.Users.FindByIdAsync(UserId))
                .ReturnsAsync(user);
            _tokenServiceMock.Setup(x => x.GenerateAccessToken(user))
                .ReturnsAsync("new-access-token");
            _tokenServiceMock.Setup(x => x.GenerateRefreshToken())
                .Returns("new-refresh-token");
            _tokenServiceMock.Setup(x => x.GetRemainingTtl(UsedAccessToken))
                .Returns(TimeSpan.FromMinutes(10));

            var command = new RefreshCommand(TokenJti, UserId, UsedAccessToken, UsedRefreshToken);

            // Act
            var result = await _sut.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.AccessToken.Should().Be("new-access-token");
            result.RefreshToken.Should().Be("new-refresh-token");
        }

        [Fact]
        public async Task Handle_BlacklistedJti_ThrowsUnauthorizedException()
        {
            // Arrange
            _cacheMock.Setup(x => x.ExistsAsync($"blacklist:jti:{TokenJti}"))
                .ReturnsAsync(true);

            var command = new RefreshCommand(TokenJti, UserId, UsedAccessToken, UsedRefreshToken);

            // Act
            var act = () => _sut.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<UnauthorizedException>()
                .Where(e => e.ErrorCode == ErrorCodes.Unauthorized);
        }

        [Fact]
        public async Task Handle_RefreshTokenMismatch_ThrowsUnauthorizedException()
        {
            // Arrange
            _cacheMock.Setup(x => x.ExistsAsync($"blacklist:jti:{TokenJti}"))
                .ReturnsAsync(false);
            _cacheMock.Setup(x => x.GetAsync($"refresh:{UserId}"))
                .ReturnsAsync("different-token");

            var command = new RefreshCommand(TokenJti, UserId, UsedAccessToken, UsedRefreshToken);

            // Act
            var act = () => _sut.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<UnauthorizedException>()
                .Where(e => e.ErrorCode == ErrorCodes.Unauthorized);
        }

        [Fact]
        public async Task Handle_UserNotFound_ThrowsUnauthorizedException()
        {
            // Arrange
            _cacheMock.Setup(x => x.ExistsAsync($"blacklist:jti:{TokenJti}"))
                .ReturnsAsync(false);
            _cacheMock.Setup(x => x.GetAsync($"refresh:{UserId}"))
                .ReturnsAsync(UsedRefreshToken);
            _uowMock.Setup(x => x.Users.FindByIdAsync(UserId))
                .ReturnsAsync((User?)null);

            var command = new RefreshCommand(TokenJti, UserId, UsedAccessToken, UsedRefreshToken);

            // Act
            var act = () => _sut.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<UnauthorizedException>()
                .Where(e => e.ErrorCode == ErrorCodes.Unauthorized);
        }

        [Fact]
        public async Task Handle_ValidRefresh_BlacklistsOldAccessToken()
        {
            // Arrange
            var user = CreateTestUser();
            var remainingTtl = TimeSpan.FromMinutes(15);

            _cacheMock.Setup(x => x.ExistsAsync($"blacklist:jti:{TokenJti}"))
                .ReturnsAsync(false);
            _cacheMock.Setup(x => x.GetAsync($"refresh:{UserId}"))
                .ReturnsAsync(UsedRefreshToken);
            _uowMock.Setup(x => x.Users.FindByIdAsync(UserId))
                .ReturnsAsync(user);
            _tokenServiceMock.Setup(x => x.GenerateAccessToken(user)).ReturnsAsync("nat");
            _tokenServiceMock.Setup(x => x.GenerateRefreshToken()).Returns("nrt");
            _tokenServiceMock.Setup(x => x.GetRemainingTtl(UsedAccessToken))
                .Returns(remainingTtl);

            var command = new RefreshCommand(TokenJti, UserId, UsedAccessToken, UsedRefreshToken);

            // Act
            await _sut.Handle(command, CancellationToken.None);

            // Assert
            _cacheMock.Verify(x => x.SetAsync(
                $"blacklist:jti:{TokenJti}",
                "revoke",
                remainingTtl), Times.Once);
        }
    }
}
