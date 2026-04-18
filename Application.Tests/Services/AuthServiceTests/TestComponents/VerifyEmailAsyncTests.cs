using Application.DTOs.Auth;
using Application.DTOs.Services.MailSender;
using AutoFixture;
using Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace Application.Tests.Services.AuthServiceTests.TestComponents
{
    public class VerifyEmailAsyncTests : AuthServiceTests
    {

        [Fact]
        public async Task VerifyEmailAsync_ShouldThrowException_WhenWalletSignatureInvalid()
        {
            // Arange
            var emailVerify = _fixture.Create<EmailVerifyDto>();
            var ws = emailVerify.WalletSignature!;

            _walletVerifyService.Setup(x => x.VerifySignature(ws.Message, ws.Signature, ws.Address)).Returns(false);

            // Act
            Func<Task> act = async () => await _sut.VerifyEmailAsync(emailVerify);

            // Assert
            await act.Should().ThrowAsync<Exception>().WithMessage("Cannot verify wallet");
            _walletVerifyService.Verify(x => x.VerifySignature(ws.Message, ws.Signature, ws.Address), Times.Once);
            _userManager.Verify(x => x.FindByNameAsync(ws.Address), Times.Never);
        }

        [Fact]
        public async Task VerifyEmailAsync_ShouldThrowException_WhenEmailAlreadyUsed()
        {
            // Arrange
            var emailVerify = _fixture.Create<EmailVerifyDto>();
            var ws = emailVerify.WalletSignature!;
            var existingUser = _fixture.Build<User>()
                .With(u => u.EmailConfirmed, true)
                .Create();

            _walletVerifyService
                .Setup(x => x.VerifySignature(ws.Message, ws.Signature, ws.Address))
                .Returns(true);
            _userManager
                .Setup(x => x.FindByNameAsync(ws.Address))
                .ReturnsAsync(existingUser);

            // Act
            Func<Task> act = async () => await _sut.VerifyEmailAsync(emailVerify);

            // Assert
            await act.Should().ThrowAsync<Exception>().WithMessage("Email already used");
            _userManager.Verify(x => x.FindByNameAsync(ws.Address), Times.Once);
            _mailRepo.Verify(x => x.GenerateSecureOTP(), Times.Never);
        }

        [Fact]
        public async Task VerifyEmailAsync_ShouldCreateUserAndSendMail_WhenUserNotExist()
        {
            // Arrange
            var emailVerify = _fixture.Create<EmailVerifyDto>();
            var ws = emailVerify.WalletSignature!;
            var otp = _fixture.Create<string>();

            _walletVerifyService
                .Setup(x => x.VerifySignature(ws.Message, ws.Signature, ws.Address))
                .Returns(true);
            _userManager
                .Setup(x => x.FindByNameAsync(ws.Address))
                .ReturnsAsync((User?)null);
            _userManager
                .Setup(x => x.CreateAsync(It.IsAny<User>()))
                .ReturnsAsync(IdentityResult.Success);
            _mailRepo
                .Setup(x => x.GenerateSecureOTP())
                .Returns(otp);
            _mailRepo
                .Setup(x => x.SaveOTPAsync(It.IsAny<string>(), otp))
                .Returns(Task.CompletedTask);
            _mailService
                .Setup(x => x.SendEmailAsync(It.IsAny<MailRequest>()))
                .Returns(Task.CompletedTask);
            _userManager
                .Setup(x => x.UpdateAsync(It.IsAny<User>()))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _sut.VerifyEmailAsync(emailVerify);

            // Assert
            result.Should().NotBeNullOrEmpty();
            _userManager.Verify(x => x.CreateAsync(It.IsAny<User>()), Times.Once);
            _mailRepo.Verify(x => x.GenerateSecureOTP(), Times.Once);
            _mailRepo.Verify(x => x.SaveOTPAsync(It.IsAny<string>(), otp), Times.Once);
            _mailService.Verify(x => x.SendEmailAsync(It.IsAny<MailRequest>()), Times.Once);
            _userManager.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task VerifyEmailAsync_ShouldSendMail_WhenUserExistButEmailNotConfirmed()
        {
            // Arrange
            var emailVerify = _fixture.Create<EmailVerifyDto>();
            var ws = emailVerify.WalletSignature!;
            var existingUser = _fixture.Build<User>()
                .With(u => u.EmailConfirmed, false)
                .Create();
            var otp = _fixture.Create<string>();

            _walletVerifyService
                .Setup(x => x.VerifySignature(ws.Message, ws.Signature, ws.Address))
                .Returns(true);
            _userManager
                .Setup(x => x.FindByNameAsync(ws.Address))
                .ReturnsAsync(existingUser);
            _mailRepo
                .Setup(x => x.GenerateSecureOTP())
                .Returns(otp);
            _mailRepo
                .Setup(x => x.SaveOTPAsync(It.IsAny<string>(), otp))
                .Returns(Task.CompletedTask);
            _mailService
                .Setup(x => x.SendEmailAsync(It.IsAny<MailRequest>()))
                .Returns(Task.CompletedTask);
            _userManager
                .Setup(x => x.UpdateAsync(It.IsAny<User>()))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _sut.VerifyEmailAsync(emailVerify);

            // Assert
            result.Should().NotBeNullOrEmpty();
            _userManager.Verify(x => x.CreateAsync(It.IsAny<User>()), Times.Never); // không tạo mới
            _mailRepo.Verify(x => x.GenerateSecureOTP(), Times.Once);
            _mailService.Verify(x => x.SendEmailAsync(It.IsAny<MailRequest>()), Times.Once);

        }
    }
}
