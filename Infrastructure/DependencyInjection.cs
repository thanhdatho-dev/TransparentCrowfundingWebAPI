using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Interfaces.Repositories;
using Infrastructure.Contexts;
using Infrastructure.Identity;
using Infrastructure.Options;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
        {
            // Database
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            // Identity
            services.AddIdentity<AppUser, IdentityRole>(options =>
            {
                options.SignIn.RequireConfirmedEmail = true;
            })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // Redis
            services.AddSingleton<IConnectionMultiplexer>(_ =>
            {
                var config = ConfigurationOptions.Parse("localhost:6379");
                return ConnectionMultiplexer.Connect(config);
            });

            // Configurations
            services.Configure<MailSettings>(configuration.GetSection("MailSettings"));
            services.Configure<JWT>(configuration.GetSection("JWT"));

            // Repositories
            services.AddScoped(typeof(IGenericRepositoryAsync<>), typeof(GenericRepositoryAsync<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IUserRepository, UserRepository>();

            // Services
            services.AddScoped<ICacheService, CacheService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IWalletSignatureVerifier, WalletSignatureVerifier>();

            return services;
        }
    }
}
