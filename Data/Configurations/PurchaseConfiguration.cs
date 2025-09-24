using CarDealership.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarDealership.Data.Configurations;

public class PurchaseConfiguration : IEntityTypeConfiguration<Purchase>
{
    public void Configure(EntityTypeBuilder<Purchase> entity)
    {
        entity.HasKey(p => p.Id);
        entity.Property(p => p.PriceAtSale).HasColumnType("decimal(18,2)");
        entity.HasIndex(p => new { p.UserId, p.VehicleId });
    }
}


