using Application.DTOs.Services.WalletSignature;
using AutoFixture;
using Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using MockQueryable;
using Moq;

namespace Application.Tests.Services.AuthServiceTests.TestComponents
{
    public class LoginAsyncTests : AuthServiceTests
    {
        private void SetupValidSignature(WalletSignatureDto ws) =>
        _walletVerifyService
            .Setup(x => x.VerifySignature(ws.Message, ws.Signature, ws.Address))
            .Returns(true);

        private void SetupInvalidSignature(WalletSignatureDto ws) =>
            _walletVerifyService
                .Setup(x => x.VerifySignature(ws.Message, ws.Signature, ws.Address))
                .Returns(false);

        private void SetupUserQuery(User? user)
        {
            var userList = user != null
                ? [user]
                : new List<User>();

            var mockUsers = userList.BuildMock();

            _userManager
                .Setup(x => x.Users)
                .Returns(mockUsers);
        }

        [Fact]
        public async Task LoginAsync_ShouldThrow_WhenWalletSignatureInvalid()
        {
            // Arrange
            var walletSignature = _fixture.Create<WalletSignatureDto>();
            SetupInvalidSignature(walletSignature);

            // Act
            Func<Task> act = async () => await _sut.LoginAsync(walletSignature);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("Wallet verification failed");

            _walletVerifyService.Verify(
                x => x.VerifySignature(walletSignature.Message, walletSignature.Signature, walletSignature.Address),
                Times.Once);
            _userManager.Verify(x => x.Users, Times.Never);
        }

        [Fact]
        public async Task LoginAsync_ShouldThrow_WhenUserNotFound()
        {
            // Arrange
            var walletSignature = _fixture.Create<WalletSignatureDto>();
            SetupValidSignature(walletSignature);
            SetupUserQuery(null);

            // Act
            Func<Task> act = async () => await _sut.LoginAsync(walletSignature);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("User not found");

            _tokenService.Verify(x => x.GenerateAccessToken(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task LoginAsync_ShouldThrow_WhenEmailNotConfirmed()
        {
            // Arrange
            var walletSignature = _fixture.Create<WalletSignatureDto>();
            var user = _fixture.Build<User>()
                .With(u => u.EmailConfirmed, false)
                .With(u => u.UserName, walletSignature.Address)
                .Create();

            SetupValidSignature(walletSignature);
            SetupUserQuery(user);

            // Act
            Func<Task> act = async () => await _sut.LoginAsync(walletSignature);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("Email verification needed");

            _tokenService.Verify(x => x.GenerateAccessToken(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnLoginDto_WhenSuccess()
        {
            // Arrange
            var walletSignature = _fixture.Create<WalletSignatureDto>();
            var user = _fixture.Build<User>()
                .With(u => u.EmailConfirmed, true)
                .With(u => u.UserName, walletSignature.Address)
                .Create();
            var accessToken = _fixture.Create<string>();
            var refreshToken = _fixture.Create<string>();

            SetupValidSignature(walletSignature);
            SetupUserQuery(user);

            _tokenService
                .Setup(x => x.GenerateAccessToken(user))
                .Returns(accessToken);
            _tokenService
                .Setup(x => x.GenerateRefreshToken())
                .Returns(refreshToken);
            _tokenRepo
                .Setup(x => x.SaveRefreshTokenAsync(user.Id, refreshToken, TimeSpan.FromDays(7)))
                .Returns(Task.CompletedTask);
            _tokenService
                .Setup(x => x.SetRefreshTokenCookie(refreshToken, It.IsAny<DateTime>()))
                .Returns(Task.CompletedTask);
            _userManager
                .Setup(x => x.UpdateAsync(user))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _sut.LoginAsync(walletSignature);

            // Assert
            result.Should().NotBeNull();
            result.UserId.Should().Be(user.Id);
            result.AccessToken.Should().Be(accessToken);
            result.RefreshToken.Should().Be(refreshToken);
            result.ExpiryRefreshTokenTime.Should().BeCloseTo(DateTime.UtcNow.AddDays(7), TimeSpan.FromSeconds(1));

            _tokenService.Verify(x => x.GenerateAccessToken(user), Times.Once);
            _tokenService.Verify(x => x.GenerateRefreshToken(), Times.Once);
            _tokenRepo.Verify(x => x.SaveRefreshTokenAsync(user.Id, refreshToken, TimeSpan.FromDays(7)), Times.Once);
            _tokenService.Verify(x => x.SetRefreshTokenCookie(refreshToken, It.IsAny<DateTime>()), Times.Once);
            _userManager.Verify(x => x.UpdateAsync(user), Times.Once);
        }
    }
}
