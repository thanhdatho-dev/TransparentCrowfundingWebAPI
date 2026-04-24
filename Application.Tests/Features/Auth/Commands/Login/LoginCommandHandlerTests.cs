using Application.DTOs.AuthDTOs;
using Application.Features.Auth.Commands.Login;
using Application.Interfaces.Services;
using Domain.Constants;
using Domain.Entities;
using Domain.Exceptions.DomainExceptions;
using Domain.Interfaces.Repositories;
using Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace Application.Tests.Features.Auth.Commands.Login
{
    public class LoginCommandHandlerTests
    {
        private readonly Mock<ICacheService> _cacheMock = new();
        private readonly Mock<IWalletSignatureVerifier> _walletVerifierMock = new();
        private readonly Mock<IUnitOfWork> _uowMock = new();
        private readonly Mock<ITokenService> _tokenServiceMock = new();
        private readonly LoginCommandHandler _sut;

        private const string ValidWallet = "0x1234567890abcdef1234567890abcdef12345678";
        private const string Message = "Sign this message";
        private const string Signature = "0xsignature123";

        public LoginCommandHandlerTests()
        {
            _sut = new LoginCommandHandler(
                _cacheMock.Object,
                _walletVerifierMock.Object,
                _uowMock.Object,
                _tokenServiceMock.Object);
        }

        private User CreateTestUser()
        {
            var wallet = WalletAddress.Create(ValidWallet);
            var email = Email.Create("test@example.com");
            return User.Register(wallet, email);
        }

        [Fact]
        public async Task Handle_ValidSignatureAndUser_ReturnsLoginDto()
        {
            // Arrange
            var user = CreateTestUser();
            var normalizedWallet = ValidWallet.ToLowerInvariant();

            _walletVerifierMock.Setup(x => x.VerifySignature(Message, Signature, normalizedWallet))
                .Returns(true);
            _uowMock.Setup(x => x.Users.FindByNameAsync(normalizedWallet))
                .ReturnsAsync(user);
            _tokenServiceMock.Setup(x => x.GenerateAccessToken(user))
                .Returns("access-token");
            _tokenServiceMock.Setup(x => x.GenerateRefreshToken())
                .Returns("refresh-token");

            var command = new LoginCommand(ValidWallet, Message, Signature);

            // Act
            var result = await _sut.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.UserId.Should().Be(user.Id);
            result.AccessToken.Should().Be("access-token");
            result.RefreshToken.Should().Be("refresh-token");
        }

        [Fact]
        public async Task Handle_InvalidSignature_ThrowsBusinessRuleViolation()
        {
            // Arrange
            _walletVerifierMock.Setup(x => x.VerifySignature(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(false);

            var command = new LoginCommand(ValidWallet, Message, Signature);

            // Act
            var act = () => _sut.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<BusinessRuleViolationException>()
                .Where(e => e.ErrorCode == ErrorCodes.InvalidWallet);
        }

        [Fact]
        public async Task Handle_UserNotFound_ThrowsEntityNotFoundException()
        {
            // Arrange
            var normalizedWallet = ValidWallet.ToLowerInvariant();
            _walletVerifierMock.Setup(x => x.VerifySignature(Message, Signature, normalizedWallet))
                .Returns(true);
            _uowMock.Setup(x => x.Users.FindByNameAsync(normalizedWallet))
                .ReturnsAsync((User?)null);

            var command = new LoginCommand(ValidWallet, Message, Signature);

            // Act
            var act = () => _sut.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<EntityNotFoundException>()
                .Where(e => e.ErrorCode == ErrorCodes.NotFound);
        }

        [Fact]
        public async Task Handle_ValidLogin_StoresRefreshTokenInCache()
        {
            // Arrange
            var user = CreateTestUser();
            var normalizedWallet = ValidWallet.ToLowerInvariant();

            _walletVerifierMock.Setup(x => x.VerifySignature(Message, Signature, normalizedWallet))
                .Returns(true);
            _uowMock.Setup(x => x.Users.FindByNameAsync(normalizedWallet))
                .ReturnsAsync(user);
            _tokenServiceMock.Setup(x => x.GenerateAccessToken(user)).Returns("at");
            _tokenServiceMock.Setup(x => x.GenerateRefreshToken()).Returns("rt");

            var command = new LoginCommand(ValidWallet, Message, Signature);

            // Act
            await _sut.Handle(command, CancellationToken.None);

            // Assert
            _cacheMock.Verify(x => x.DeleteAsync($"refresh:{user.Id}"), Times.Once);
            _cacheMock.Verify(x => x.SetAsync(
                $"refresh:{user.Id}",
                "rt",
                TimeSpan.FromDays(AuthConstants.RefreshTokenExpiryDays)), Times.Once);
        }
    }
}
