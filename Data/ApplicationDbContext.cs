using Microsoft.EntityFrameworkCore;
using CarDealership.Models;

namespace CarDealership.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Passcode> Passcodes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Phone).IsUnique();
            entity.Property(e => e.Phone).IsRequired().HasMaxLength(20);
        });

        modelBuilder.Entity<Passcode>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId).IsUnique();
            entity.Property(e => e.Hash).IsRequired().HasMaxLength(255);
            
            entity.HasOne(e => e.User)
                .WithOne(u => u.Passcode)
                .HasForeignKey<Passcode>(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
