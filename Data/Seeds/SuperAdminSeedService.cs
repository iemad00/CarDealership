using Microsoft.EntityFrameworkCore;
using CarDealership.Data;
using CarDealership.Models;
using CarDealership.Services;

namespace CarDealership.Data.Seeds;

public class SuperAdminSeedService : ISeedService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<SuperAdminSeedService> _logger;
    private readonly IPasscodeHashService _passcodeHashService;

    public SuperAdminSeedService(
        ApplicationDbContext context,
        ILogger<SuperAdminSeedService> logger,
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
            var phone = "0542306039";
            var email = "emad@cardealership.com";
            var name = "Emad Saleh";

            // Check if super admin already exists (by phone OR email)
            var existingSuperAdmin = await _context.AdminUsers
                .FirstOrDefaultAsync(u => u.Phone == phone || u.Email == email);

            if (existingSuperAdmin != null)
            {
                _logger.LogInformation("Super admin already exists: {Phone} or {Email}", phone, email);
                return;
            }

            // Create super admin user
            var superAdmin = new AdminUser
            {
                Phone = phone,
                Name = name,
                Email = email,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.AdminUsers.Add(superAdmin);
            await _context.SaveChangesAsync();

            var passcodeHash = _passcodeHashService.HashAdminPasscode("123456");
            var passcode = new Passcode
            {
                UserId = superAdmin.Id,
                UserType = "AdminUser",
                Hash = passcodeHash,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                FailedAttempts = 0
            };

            _context.Passcodes.Add(passcode);
            await _context.SaveChangesAsync();

            // Check if SuperAdmin role already exists, if not create it
            var superAdminRole = await _context.Roles
                .FirstOrDefaultAsync(r => r.Name == "SuperAdmin");

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

            // Create permissions
            await SeedPermissionsAsync();

            // Assign all permissions to Super Admin role (only if not already assigned)
            var permissions = await _context.Permissions.ToListAsync();
            foreach (var permission in permissions)
            {
                var existingRolePermission = await _context.RolePermissions
                    .FirstOrDefaultAsync(rp => rp.RoleId == superAdminRole.Id && rp.PermissionId == permission.Id);
                
                if (existingRolePermission == null)
                {
                    var rolePermission = new RolePermission
                    {
                        RoleId = superAdminRole.Id,
                        PermissionId = permission.Id,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.RolePermissions.Add(rolePermission);
                }
            }

            // Assign Super Admin role to the user (only if not already assigned)
            var existingAdminUserRole = await _context.AdminUserRoles
                .FirstOrDefaultAsync(aur => aur.AdminUserId == superAdmin.Id && aur.RoleId == superAdminRole.Id);

            if (existingAdminUserRole == null)
            {
                var adminUserRole = new AdminUserRole
                {
                    AdminUserId = superAdmin.Id,
                    RoleId = superAdminRole.Id,
                    AssignedAt = DateTime.UtcNow,
                    IsActive = true
                };

                _context.AdminUserRoles.Add(adminUserRole);
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Super admin created successfully: {Name} ({Phone})", superAdmin.Name, superAdmin.Phone);
            _logger.LogInformation("Super admin default passcode: 1234 (Please change after first login)");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding super admin data");
            throw;
        }
    }

    private async Task SeedPermissionsAsync()
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
            new() { Name = "Read Roles", Description = "View roles and permissions", Resource = "roles", Action = "read", CreatedAt = DateTime.UtcNow, IsActive = true }
        };

        // Only add permissions that don't already exist
        foreach (var permission in permissionsToCreate)
        {
            var existingPermission = await _context.Permissions
                .FirstOrDefaultAsync(p => p.Resource == permission.Resource && p.Action == permission.Action);
            
            if (existingPermission == null)
            {
                _context.Permissions.Add(permission);
            }
        }

        await _context.SaveChangesAsync();
    }
}
