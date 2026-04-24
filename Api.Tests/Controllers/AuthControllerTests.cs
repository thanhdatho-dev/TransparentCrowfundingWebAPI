using API.Controllers;
using API.Models.Requests.Auth;
using Application.DTOs.AuthDTOs;
using Application.Features.Auth.Commands.ConfirmEmail;
using Application.Features.Auth.Commands.Login;
using Application.Features.Auth.Commands.Logout;
using Application.Features.Auth.Commands.Refresh;
using Application.Features.Auth.Commands.VerifyEmail;
using Application.Features.Auth.Queries.CheckWalletRegistered;
using Application.Features.Auth.Queries.GetCurrentUser;
using Application.Interfaces.Services;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Api.Tests.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<ISender> _senderMock = new();
        private readonly Mock<ICacheService> _cacheMock = new();
        private readonly AuthController _sut;

        public AuthControllerTests()
        {
            _sut = new AuthController(_senderMock.Object, _cacheMock.Object);

            // Setup default HttpContext
            var httpContext = new DefaultHttpContext();
            _sut.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        private void SetAuthenticatedUser(string userId, string? jti = null)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userId)
            };
            if (jti != null)
                claims.Add(new Claim(JwtRegisteredClaimNames.Jti, jti));

            var identity = new ClaimsIdentity(claims, "TestAuth");
            _sut.ControllerContext.HttpContext.User = new ClaimsPrincipal(identity);
        }

        [Fact]
        public async Task EmailVerify_ValidRequest_ReturnsOk()
        {
            // Arrange
            var request = new VerifyEmailRequest("test@test.com", "0x123", "msg", "sig");
            _senderMock.Setup(x => x.Send(It.IsAny<VerifyEmailCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Unit.Value);

            // Act
            var result = await _sut.EmailVerify(request, CancellationToken.None);

            // Assert
            result.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().Be("OTP sent to email");
        }

        [Fact]
        public async Task Login_ValidRequest_ReturnsOkWithTokens()
        {
            // Arrange
            var request = new LoginRequest("msg", "sig", "0x123");
            var loginDto = new LoginDto
            {
                UserId = Guid.NewGuid(),
                AccessToken = "access-token",
                RefreshToken = "refresh-token",
                ExpiryRefreshTokenTime = DateTime.UtcNow.AddDays(7)
            };
            _senderMock.Setup(x => x.Send(It.IsAny<LoginCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(loginDto);

            // Act
            var result = await _sut.Login(request, CancellationToken.None);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            // Verify refresh token cookie is set
            _sut.HttpContext.Response.Headers.Should().ContainKey("Set-Cookie");
        }

        [Fact]
        public async Task Logout_AuthenticatedUser_ReturnsOk()
        {
            // Arrange
            SetAuthenticatedUser("user-id-123", "jti-123");
            _sut.HttpContext.Request.Headers.Authorization = "Bearer valid-access-token";

            _senderMock.Setup(x => x.Send(It.IsAny<LogoutCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Unit.Value);

            // Act
            var result = await _sut.Logout(CancellationToken.None);

            // Assert
            result.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().Be("Logged out");
        }

        [Fact]
        public async Task Logout_NoUserId_ReturnsUnauthorized()
        {
            // Arrange — no user claims set (anonymous)

            // Act
            var result = await _sut.Logout(CancellationToken.None);

            // Assert
            result.Should().BeOfType<UnauthorizedResult>();
        }

        [Fact]
        public async Task ConfirmEmail_ValidRequest_ReturnsOk()
        {
            // Arrange
            var request = new ConfirmEmailRequest("test@test.com", "0x123", "123456");
            _senderMock.Setup(x => x.Send(It.IsAny<ConfirmEmailCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Unit.Value);

            // Act
            var result = await _sut.ConfirmEmail(request, CancellationToken.None);

            // Assert
            result.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().Be("Verify email successfully");
        }

        [Fact]
        public async Task GetCurrentUser_AuthenticatedUser_ReturnsOkWithProfile()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            SetAuthenticatedUser(userId);

            var profile = new UserProfileDto
            {
                Id = Guid.Parse(userId),
                WalletAddress = "0x123",
                Email = "test@test.com",
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };
            _senderMock.Setup(x => x.Send(It.IsAny<GetCurrentUserQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(profile);

            // Act
            var result = await _sut.GetCurrentUser(CancellationToken.None);

            // Assert
            result.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().Be(profile);
        }

        [Fact]
        public async Task CheckWalletRegistered_ValidWallet_ReturnsOkWithResult()
        {
            // Arrange
            _senderMock.Setup(x => x.Send(It.IsAny<CheckWalletRegisteredQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _sut.CheckWalletRegistered("0x123", CancellationToken.None);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
