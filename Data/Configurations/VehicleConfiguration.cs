using CarDealership.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarDealership.Data.Configurations;

public class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> entity)
    {
        entity.HasKey(v => v.Id);
        entity.Property(v => v.Make).IsRequired().HasMaxLength(50);
        entity.Property(v => v.Model).IsRequired().HasMaxLength(50);
        entity.Property(v => v.Year).IsRequired();
        entity.Property(v => v.Price).HasColumnType("decimal(18,2)");
        entity.Property(v => v.Color).HasMaxLength(30);
        entity.Property(v => v.Description).HasMaxLength(1000);
        entity.Property(v => v.Kilometers).IsRequired();
    }
}


