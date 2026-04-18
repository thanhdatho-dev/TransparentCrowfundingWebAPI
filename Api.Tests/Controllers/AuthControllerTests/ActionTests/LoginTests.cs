using Application.DTOs.Auth;
using Application.DTOs.Services.WalletSignature;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Api.Tests.Controllers.AuthControllerTests.ActionTests
{
    public class LoginTests : AuthControllerTests
    {
        [Theory]
        [InlineData
        (
            "0xe8d0e13c779b864c4ad1ffea63656b18d337921d",
            "Sign in to My MetaMask Connect EVM dapp",
            "0xbc726edb5105c4be3abe246db44795e6c20e450f74012f4f438d59afa84ef2cd2d92200568f8f4b79b231b6620ef0b9ac74955657ca80c064d659a1c5cce283c1c"
        )]
        public async Task Login_ShoudReturnOk_WhenWalletSignatureValid(string address, string message, string signature)
        {
            // Arrange
            var loginDto = _fixture.Create<LoginDto>();
            var walletSignature = new WalletSignatureDto
            {
                Address = address,
                Message = message,
                Signature = signature
            };

            _serviceMock.Setup(x => x.LoginAsync(It.IsAny<WalletSignatureDto>()))
                .ReturnsAsync(loginDto);

            // Act
            var result = await _sut.Login(walletSignature);

            // Assert
            result.Should().NotBeNull();
            result.Should().NotBeAssignableTo<BadRequestObjectResult>();
            result.Should().BeAssignableTo<OkObjectResult>();
        }

        [Theory]
        [InlineData
        (
            "",
            "Sign in to My MetaMask Connect EVM dapp",
            "0xbc726edb5105c4be3abe246db44795e6c20e450f74012f4f438d59afa84ef2cd2d92200568f8f4b79b231b6620ef0b9ac74955657ca80c064d659a1c5cce283c1c"
        )]
        [InlineData
        (
            "0xe8d0e13c779b864c4ad1ffea63656b18d337921d",
            "",
            "0xbc726edb5105c4be3abe246db44795e6c20e450f74012f4f438d59afa84ef2cd2d92200568f8f4b79b231b6620ef0b9ac74955657ca80c064d659a1c5cce283c1c"
        )]
        [InlineData
        (
            "0xe8d0e13c779b864c4ad1ffea63656b18d337921d",
            "Sign in to My MetaMask Connect EVM dapp",
            ""
        )]
        public async Task Login_ShoudReturnBadRequest_WhenWalletSignatureIncomplete(string address, string message, string signature)
        {
            // Arrange
            var loginDto = _fixture.Create<LoginDto>();
            var walletSignature = new WalletSignatureDto
            {
                Address = address,
                Message = message,
                Signature = signature
            };

            _serviceMock.Setup(x => x.LoginAsync(It.IsAny<WalletSignatureDto>()))
                .ReturnsAsync(loginDto);

            // Act
            var result = await _sut.Login(walletSignature);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<BadRequestObjectResult>();
            result.Should().NotBeAssignableTo<OkObjectResult>();
        }

        [Fact]
        public async Task Login_ShouldReturnBadRequest_WhenModelStateInvalid()
        {
            // Arrange
            var ws = new WalletSignatureDto();

            _sut.ModelState.AddModelError("Address", "Address is required");

            // Act
            var result = await _sut.Login(ws);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }
    }
}
