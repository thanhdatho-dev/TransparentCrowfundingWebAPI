using Domain.Entities;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Contexts
{
    public partial class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<AppUser>(options)
    {
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            string adminRoleId = "31cb0cfb-5810-48df-b4ac-0a8248e99f3f";
            string creatorRoleId = "7bcfb2d2-2600-4be6-b331-d71d000a6042";
            string userRoleId = "5365b229-f6a8-4fb8-817a-59f8597ff5f4";

            builder.Entity<IdentityRole>().HasData(
                new IdentityRole { Id = adminRoleId, Name = "Admin", NormalizedName = "ADMIN", ConcurrencyStamp = null },
                new IdentityRole { Id = creatorRoleId, Name = "Creator", NormalizedName = "CREATOR", ConcurrencyStamp = null },
                new IdentityRole { Id = userRoleId, Name = "User", NormalizedName = "USER" , ConcurrencyStamp = null }
            );
        }
    }
}
