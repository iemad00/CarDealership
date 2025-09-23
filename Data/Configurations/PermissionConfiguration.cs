using CarDealership.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarDealership.Data.Configurations;

public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> entity)
    {
        entity.HasKey(e => e.Id);
        entity.HasIndex(e => new { e.Resource, e.Action }).IsUnique();
        entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
        entity.Property(e => e.Description).HasMaxLength(200);
        entity.Property(e => e.Resource).IsRequired().HasMaxLength(50);
        entity.Property(e => e.Action).IsRequired().HasMaxLength(50);
    }
}


