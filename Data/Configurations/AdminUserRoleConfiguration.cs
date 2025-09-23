using CarDealership.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarDealership.Data.Configurations;

public class AdminUserRoleConfiguration : IEntityTypeConfiguration<AdminUserRole>
{
    public void Configure(EntityTypeBuilder<AdminUserRole> entity)
    {
        entity.HasKey(e => e.Id);
        entity.HasIndex(e => e.AdminUserId).IsUnique();

        entity.HasOne(e => e.AdminUser)
            .WithOne(u => u.AdminUserRole)
            .HasForeignKey<AdminUserRole>(e => e.AdminUserId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasOne(e => e.Role)
            .WithMany(r => r.AdminUserRoles)
            .HasForeignKey(e => e.RoleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}


