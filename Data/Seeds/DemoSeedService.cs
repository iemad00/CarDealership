using Microsoft.EntityFrameworkCore;
using CarDealership.Data;
using CarDealership.Models;
using CarDealership.Services;

namespace CarDealership.Data.Seeds;

public class DemoSeedService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DemoSeedService> _logger;
    private readonly IPasscodeHashService _passcodeHashService;

    public DemoSeedService(
        ApplicationDbContext context,
        ILogger<DemoSeedService> logger,
        IPasscodeHashService passcodeHashService)
    {
        _context = context;
        _logger = logger;
        _passcodeHashService = passcodeHashService;
    }

    public async Task SeedAsync()
    {
        try
        {
            await SeedVehiclesAsync();
            await SeedAdminAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running demo seed");
            throw;
        }
    }

    private async Task SeedVehiclesAsync()
    {
        var existingCount = await _context.Vehicles.CountAsync();
        var target = 20;
        if (existingCount >= target)
        {
            _logger.LogInformation("Vehicles already seeded: {Count}", existingCount);
            return;
        }

        var makes = new[] { "Toyota", "Honda", "Nissan", "Ford", "Chevrolet", "BMW", "Mercedes", "Lexus" };
        var models = new[] { "Camry", "Corolla", "Civic", "Accord", "Altima", "Focus", "Impala", "3 Series", "C-Class", "ES" };
        var colors = new[] { "White", "Black", "Silver", "Blue", "Red", "Gray" };
        var rand = new Random();

        var toAdd = new List<Vehicle>();
        for (int i = existingCount; i < target; i++)
        {
            var make = makes[rand.Next(makes.Length)];
            var model = models[rand.Next(models.Length)];
            var year = rand.Next(2015, DateTime.UtcNow.Year + 1);
            var price = rand.Next(30000, 200000);
            var km = rand.Next(0, 200000);
            var color = colors[rand.Next(colors.Length)];

            toAdd.Add(new Vehicle
            {
                Make = make,
                Model = model,
                Year = year,
                Price = price,
                Kilometers = km,
                Color = color,
                Description = "Seeded vehicle",
                IsActive = true
            });
        }

        if (toAdd.Count > 0)
        {
            _context.Vehicles.AddRange(toAdd);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Seeded {Count} vehicles", toAdd.Count);
        }
    }

    private async Task SeedAdminAsync()
    {
        const string phone = "0555555555";
        const string email = "seed-admin@cardealership.com";
        const string name = "Seed Admin";

        var admin = await _context.AdminUsers.FirstOrDefaultAsync(u => u.Phone == phone || u.Email == email);
        if (admin == null)
        {
            admin = new AdminUser
            {
                Phone = phone,
                Name = name,
                Email = email,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
            _context.AdminUsers.Add(admin);
            await _context.SaveChangesAsync();

            var passcodeHash = _passcodeHashService.HashAdminPasscode("123456");
            var passcode = new Passcode
            {
                UserId = admin.Id,
                UserType = "AdminUser",
                Hash = passcodeHash,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                FailedAttempts = 0
            };
            _context.Passcodes.Add(passcode);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Created seed admin {Phone}", phone);
        }

        // Ensure role and permissions
        var superAdminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "SuperAdmin");
        if (superAdminRole == null)
        {
            superAdminRole = new Role
            {
                Name = "SuperAdmin",
                Description = "Super Administrator with full system access",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
            _context.Roles.Add(superAdminRole);
            await _context.SaveChangesAsync();
        }

        await EnsurePermissionsAsync();

        // Assign all permissions to role
        var permissions = await _context.Permissions.ToListAsync();
        foreach (var permission in permissions)
        {
            var has = await _context.RolePermissions.FirstOrDefaultAsync(rp => rp.RoleId == superAdminRole.Id && rp.PermissionId == permission.Id);
            if (has == null)
            {
                _context.RolePermissions.Add(new RolePermission
                {
                    RoleId = superAdminRole.Id,
                    PermissionId = permission.Id,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        // Assign role to admin
        var adminRole = await _context.AdminUserRoles.FirstOrDefaultAsync(ur => ur.AdminUserId == admin.Id && ur.RoleId == superAdminRole.Id);
        if (adminRole == null)
        {
            _context.AdminUserRoles.Add(new AdminUserRole
            {
                AdminUserId = admin.Id,
                RoleId = superAdminRole.Id,
                AssignedAt = DateTime.UtcNow,
                IsActive = true
            });
        }

        await _context.SaveChangesAsync();
    }

    private async Task EnsurePermissionsAsync()
    {
        var permissionsToCreate = new List<Permission>
        {
            new() { Name = "Create Admin Users", Description = "Create new admin users", Resource = "admin_users", Action = "create", CreatedAt = DateTime.UtcNow, IsActive = true },
            new() { Name = "Read Admin Users", Description = "View admin users", Resource = "admin_users", Action = "read", CreatedAt = DateTime.UtcNow, IsActive = true },
            new() { Name = "Update Admin Users", Description = "Modify admin users and roles", Resource = "admin_users", Action = "update", CreatedAt = DateTime.UtcNow, IsActive = true },
            new() { Name = "Delete Admin Users", Description = "Deactivate admin users", Resource = "admin_users", Action = "delete", CreatedAt = DateTime.UtcNow, IsActive = true },
            new() { Name = "Create Roles", Description = "Create new roles", Resource = "roles", Action = "create", CreatedAt = DateTime.UtcNow, IsActive = true },
            new() { Name = "Read Roles", Description = "View roles and permissions", Resource = "roles", Action = "read", CreatedAt = DateTime.UtcNow, IsActive = true },
            new() { Name = "Update Roles", Description = "Modify roles and permissions", Resource = "roles", Action = "update", CreatedAt = DateTime.UtcNow, IsActive = true },
            new() { Name = "Delete Roles", Description = "Delete roles", Resource = "roles", Action = "delete", CreatedAt = DateTime.UtcNow, IsActive = true },
            new() { Name = "Create Vehicles", Description = "Add new vehicles", Resource = "vehicles", Action = "create", CreatedAt = DateTime.UtcNow, IsActive = true },
            new() { Name = "Read Vehicles", Description = "View vehicles", Resource = "vehicles", Action = "read", CreatedAt = DateTime.UtcNow, IsActive = true },
            new() { Name = "Update Vehicles", Description = "Modify vehicles", Resource = "vehicles", Action = "update", CreatedAt = DateTime.UtcNow, IsActive = true },
            new() { Name = "Delete Vehicles", Description = "Delete vehicles", Resource = "vehicles", Action = "delete", CreatedAt = DateTime.UtcNow, IsActive = true },
            new() { Name = "Create Sales", Description = "Create sales records", Resource = "sales", Action = "create", CreatedAt = DateTime.UtcNow, IsActive = true },
            new() { Name = "Read Sales", Description = "View sales records", Resource = "sales", Action = "read", CreatedAt = DateTime.UtcNow, IsActive = true },
            new() { Name = "Update Sales", Description = "Modify sales records", Resource = "sales", Action = "update", CreatedAt = DateTime.UtcNow, IsActive = true },
            new() { Name = "Delete Sales", Description = "Delete sales records", Resource = "sales", Action = "delete", CreatedAt = DateTime.UtcNow, IsActive = true },
            new() { Name = "Read Permissions", Description = "View permissions", Resource = "permissions", Action = "read", CreatedAt = DateTime.UtcNow, IsActive = true },
            new() { Name = "Read Customers", Description = "View customers", Resource = "customers", Action = "read", CreatedAt = DateTime.UtcNow, IsActive = true }
        };

        foreach (var permission in permissionsToCreate)
        {
            var exists = await _context.Permissions.FirstOrDefaultAsync(p => p.Resource == permission.Resource && p.Action == permission.Action);
            if (exists == null)
            {
                _context.Permissions.Add(permission);
            }
        }

        await _context.SaveChangesAsync();
    }
}
