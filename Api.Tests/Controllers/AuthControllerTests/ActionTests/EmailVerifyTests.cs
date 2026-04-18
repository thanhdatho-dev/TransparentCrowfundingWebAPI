using Application.DTOs.Auth;
using Application.DTOs.Services.WalletSignature;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Api.Tests.Controllers.AuthControllerTests.ActionTests
{
    public class EmailVerifyTests : AuthControllerTests
    {
        [Theory]
        [InlineData
        (
            "valid-email",
            "valid-address",
            "valid-message",
            "valid-signature"
        )]
        public async Task EmailVerify_ShouldReturnOk_WhenReqestValid(
            string email, string address, string message, string signature)
        {
            // Arrange
            var emailVerifyDto = new EmailVerifyDto
            {
                Email = email,
                WalletSignature = new WalletSignatureDto
                {
                    Address = address,
                    Message = message,
                    Signature = signature
                }
            };
            _serviceMock.Setup(x => x.VerifyEmailAsync(It.IsAny<EmailVerifyDto>()))
                .ReturnsAsync("value");

            //Act
            var result = await _sut.EmailVerify(emailVerifyDto);

            //Assert
            result.Should().NotBeNull();
            result.Should().NotBeAssignableTo<BadRequestObjectResult>();
            result.Should().BeAssignableTo<OkObjectResult>();
        }

        [Theory]
        [InlineData
        (
            "",
            "valid-address",
            "valid-message",
            "valid-signature"
        )]
        public async Task EmailVerify_ShouldReturnBadRequest_WhenEmailMissing(
            string email, string address, string message, string signature)
        {
            // Arrange
            var emailVerifyDto = new EmailVerifyDto
            {
                Email = email,
                WalletSignature = new WalletSignatureDto
                {
                    Address = address,
                    Message = message,
                    Signature = signature
                }
            };
            _serviceMock.Setup(x => x.VerifyEmailAsync(It.IsAny<EmailVerifyDto>()))
                .ReturnsAsync("value");

            //Act
            var result = await _sut.EmailVerify(emailVerifyDto);

            //Assert
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<BadRequestObjectResult>();
            result.Should().NotBeAssignableTo<OkObjectResult>();
        }

        [Theory]
        [InlineData
        (
            "valid-email",
            "",
            "valid-message",
            "valid-signature"
        )]
        [InlineData
        (
            "valid-email",
            "valid-address",
            "",
            "valid-signature"
        )]
        [InlineData
        (
            "valid-email",
            "valid-address",
            "valid-message",
            ""
        )]
        public async Task EmailVerify_ShouldReturnBadRequest_WhenWalletSignatureIncomplete(
            string email, string address, string message, string signature)
        {
            // Arrange
            var emailVerifyDto = new EmailVerifyDto
            {
                Email = email,
                WalletSignature = new WalletSignatureDto
                {
                    Address = address,
                    Message = message,
                    Signature = signature
                }
            };
            _serviceMock.Setup(x => x.VerifyEmailAsync(It.IsAny<EmailVerifyDto>()))
                .ReturnsAsync("value");

            //Act
            var result = await _sut.EmailVerify(emailVerifyDto);

            //Assert
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<BadRequestObjectResult>();
            result.Should().NotBeAssignableTo<OkObjectResult>();
        }

        [Fact]
        public async Task EmailVerify_ShouldReturnBadReques_WhenModelStateInvalid()
        {
            // Arrange
            var emailVerifier = new EmailVerifyDto();
            var ws = new WalletSignatureDto();
            emailVerifier.WalletSignature = ws;

            _sut.ModelState.AddModelError("Email", "Email is required");
            _sut.ModelState.AddModelError("Address", "Address is required");

            // Act
            var result = await _sut.EmailVerify(emailVerifier);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }
    }
}
