using Domain.Entities;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Contexts.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            // WalletAddress → lưu dạng string trong DB
            builder.Property(u => u.WalletAddress)
                .HasConversion(
                    vo => vo.Value,                          // C# → DB
                    dbValue => WalletAddress.Create(dbValue)) // DB → C#
                .HasMaxLength(42)
                .IsRequired();
            // Email → lưu dạng string trong DB
            builder.Property(u => u.Email)
                .HasConversion(
                    vo => vo.Value,
                    dbValue => Email.Create(dbValue))
                .HasMaxLength(256)
                .IsRequired();
        }
    }
}
