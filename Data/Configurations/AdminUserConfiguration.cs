using CarDealership.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarDealership.Data.Configurations;

public class AdminUserConfiguration : IEntityTypeConfiguration<AdminUser>
{
    public void Configure(EntityTypeBuilder<AdminUser> entity)
    {
        entity.HasKey(e => e.Id);
        entity.HasIndex(e => e.Phone).IsUnique();
        entity.HasIndex(e => e.Email).IsUnique();
        entity.Property(e => e.Phone).IsRequired().HasMaxLength(20);
        entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
        entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
    }
}


