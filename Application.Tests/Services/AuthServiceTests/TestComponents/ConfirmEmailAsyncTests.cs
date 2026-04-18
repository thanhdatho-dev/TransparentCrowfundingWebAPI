using Application.DTOs.Auth;
using AutoFixture;
using Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace Application.Tests.Services.AuthServiceTests.TestComponents
{
    public class ConfirmEmailAsyncTests : AuthServiceTests
    {

        [Fact]
        public async Task ConfirmEmailAsync_ShouldThrow_WhenUserIdIsNull()
        {
            // Arrange
            var client = _fixture.Create<EmailConfirmWithOTP>();

            // Act
            Func<Task> act = async () => await _sut.ConfirmEmailAsync(client, null!);

            // Assert
            await act.Should()
                .ThrowAsync<Exception>()
                .WithMessage("UserId is invalid");

            _mailRepo.Verify(x => x.GetOTPAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ConfirmEmailAsync_ShouldThrow_WhenUserIdIsEmpty()
        {
            // Arrange
            var client = _fixture.Create<EmailConfirmWithOTP>();

            // Act
            Func<Task> act = async () => await _sut.ConfirmEmailAsync(client, string.Empty);

            // Assert
            await act.Should()
                .ThrowAsync<Exception>()
                .WithMessage("UserId is invalid");
        }

        [Fact]
        public async Task ConfirmEmailAsync_ShouldThrow_WhenOTPNotFound()
        {
            // Arrange
            var client = _fixture.Create<EmailConfirmWithOTP>();
            var userId = _fixture.Create<string>();

            _mailRepo
                .Setup(x => x.GetOTPAsync(userId))
                .ReturnsAsync((string?)null);

            // Act
            Func<Task> act = async () => await _sut.ConfirmEmailAsync(client, userId);

            // Assert
            await act.Should()
                .ThrowAsync<Exception>()
                .WithMessage("Invalid OTP");

            _userManager.Verify(x => x.FindByIdAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ConfirmEmailAsync_ShouldThrow_WhenOTPDoesNotMatch()
        {
            // Arrange
            var client = _fixture.Create<EmailConfirmWithOTP>();
            var userId = _fixture.Create<string>();
            var differentOtp = _fixture.Create<string>(); // OTP khác với client.OTP

            _mailRepo
                .Setup(x => x.GetOTPAsync(userId))
                .ReturnsAsync(differentOtp);

            // Act
            Func<Task> act = async () => await _sut.ConfirmEmailAsync(client, userId);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("OTP does not match");

            _userManager.Verify(x => x.FindByIdAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ConfirmEmailAsync_ShouldThrow_WhenUserNotFound()
        {
            // Arrange
            var client = _fixture.Create<EmailConfirmWithOTP>();
            var userId = _fixture.Create<string>();

            _mailRepo
                .Setup(x => x.GetOTPAsync(userId))
                .ReturnsAsync(client.ClientOTP); // OTP khớp

            _userManager
                .Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync((User?)null);

            // Act
            Func<Task> act = async () => await _sut.ConfirmEmailAsync(client, userId);

            // Assert
            await act.Should()
                .ThrowAsync<Exception>()
                .WithMessage("Invalid user's request");

            _userManager.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task ConfirmEmailAsync_ShouldConfirmEmail_WhenSuccess()
        {
            // Arrange
            var client = _fixture.Create<EmailConfirmWithOTP>();
            var userId = _fixture.Create<string>();
            var user = _fixture.Build<User>()
                .With(u => u.EmailConfirmed, false)
                .Create();

            _mailRepo
                .Setup(x => x.GetOTPAsync(userId))
                .ReturnsAsync(client.ClientOTP);
            _userManager
                .Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync(user);
            _userManager
                .Setup(x => x.UpdateAsync(user))
                .ReturnsAsync(IdentityResult.Success);
            _mailRepo
                .Setup(x => x.DeleteOTPAsync(userId))
                .Returns(Task.CompletedTask);

            // Act
            await _sut.ConfirmEmailAsync(client, userId);

            // Assert
            user.EmailConfirmed.Should().BeTrue();
            _userManager.Verify(x => x.UpdateAsync(user), Times.Once);
            _mailRepo.Verify(x => x.DeleteOTPAsync(userId), Times.Once);
        }

    }
}
