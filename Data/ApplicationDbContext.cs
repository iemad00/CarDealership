using Microsoft.EntityFrameworkCore;
using CarDealership.Models;
using System.Reflection;

namespace CarDealership.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    // User entities
    public DbSet<User> Users { get; set; }
    public DbSet<Passcode> Passcodes { get; set; }
    
    // Admin entities
    public DbSet<AdminUser> AdminUsers { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }
    public DbSet<AdminUserRole> AdminUserRoles { get; set; }
    public DbSet<Vehicle> Vehicles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
