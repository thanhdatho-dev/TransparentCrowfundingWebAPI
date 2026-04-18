using Application.DTOs.Auth;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Api.Tests.Controllers.AuthControllerTests.ActionTests
{
    public class ConfirmEmailTests : AuthControllerTests
    {
        [Theory]
        [InlineData("valid-otp", "valid-userId")]
        public async Task ConfirmEmail_ShouldReturnOk_WhenRequestValid(string otp, string userId)
        {
            // Arrange
            var emailConfirmOtp = new EmailConfirmWithOTP { ClientOTP = otp };
            _serviceMock.Setup(x => x.ConfirmEmailAsync(emailConfirmOtp, userId)).Returns(Task.CompletedTask);

            // Act
            var result = await _sut.ConfirmEmail(emailConfirmOtp, userId);

            // Assert
            _serviceMock.Verify(x => x.ConfirmEmailAsync(emailConfirmOtp, userId), Times.Once);
            result.Should().NotBeNull();
            result.Should().BeOfType<OkObjectResult>();
        }

        [Theory]
        [InlineData("")]
        public async Task ConfirmEmail_ShouldReturnBadReques_WhenModelStateInvalid(string userId)
        {
            // Arrange
            _sut.ModelState.AddModelError("OTP", "OTP is required");

            // Act
            var result = await _sut.ConfirmEmail(It.IsAny<EmailConfirmWithOTP>(), userId);

            // Assert
            _serviceMock.Verify(x => x.ConfirmEmailAsync(It.IsAny<EmailConfirmWithOTP>(), userId), Times.Never);
            result.Should().NotBeNull();
            result.Should().BeOfType<BadRequestObjectResult>();
        }
    }
}
