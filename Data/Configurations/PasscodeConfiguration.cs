using CarDealership.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarDealership.Data.Configurations;

public class PasscodeConfiguration : IEntityTypeConfiguration<Passcode>
{
    public void Configure(EntityTypeBuilder<Passcode> entity)
    {
        entity.HasKey(e => e.Id);
        entity.HasIndex(e => new { e.UserId, e.UserType }).IsUnique();
        entity.Property(e => e.Hash).IsRequired().HasMaxLength(255);
        entity.Property(e => e.UserType).IsRequired().HasMaxLength(20);
    }
}


