using CarDealership.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarDealership.Data.Configurations;

public class PurchaseRequestConfiguration : IEntityTypeConfiguration<PurchaseRequest>
{
    public void Configure(EntityTypeBuilder<PurchaseRequest> entity)
    {
        entity.HasKey(p => p.Id);
        entity.HasIndex(p => new { p.UserId, p.VehicleId, p.Status });
        entity.Property(p => p.Status).IsRequired().HasMaxLength(20);
        entity.Property(p => p.QuotedPrice).HasColumnType("decimal(18,2)");
    }
}


