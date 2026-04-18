using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Services;
using Application.Services;
using AutoFixture;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace Application.Tests.Services.AuthServiceTests
{
    public class AuthServiceTests
    {
        protected readonly IFixture _fixture;

        // Mock Repo
        protected readonly Mock<UserManager<User>> _userManager;
        protected readonly Mock<IWalletSignatureVerifier> _walletVerifyService;
        protected readonly Mock<ITokenService> _tokenService;
        protected readonly Mock<ITokenRepository> _tokenRepo;
        protected readonly Mock<IMailRepository> _mailRepo;
        protected readonly Mock<IMailService> _mailService;

        // System under test
        protected readonly AuthService _sut;

        public AuthServiceTests()
        {

            _fixture = new Fixture();
            var userStoreMock = new Mock<IUserStore<User>>();
            _userManager = new Mock<UserManager<User>>(
                userStoreMock.Object,  // IUserStore - bắt buộc
                null!,                  // IOptions<IdentityOptions>
                null!,                  // IPasswordHasher
                null!,                  // IEnumerable<IUserValidator>
                null!,                  // IEnumerable<IPasswordValidator>
                null!,                  // ILookupNormalizer
                null!,                  // IdentityErrorDescriber
                null!,                  // IServiceProvider
                null!                   // ILogger
            );
            _walletVerifyService = new Mock<IWalletSignatureVerifier>();
            _tokenRepo = new Mock<ITokenRepository>();
            _tokenService = new Mock<ITokenService>();
            _mailRepo = new Mock<IMailRepository>();
            _mailService = new Mock<IMailService>();

            _sut = new AuthService(_userManager.Object, _walletVerifyService.Object, _tokenRepo.Object, _tokenService.Object, _mailRepo.Object, _mailService.Object);
        }
    }
}
