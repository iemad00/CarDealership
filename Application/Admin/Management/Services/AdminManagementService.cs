using Microsoft.EntityFrameworkCore;
using CarDealership.Data;
using CarDealership.Models;
using CarDealership.Models.DTOs.Admin;

namespace CarDealership.Services.Admin;

public class AdminManagementService : IAdminManagementService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AdminManagementService> _logger;

    public AdminManagementService(
        ApplicationDbContext context,
        ILogger<AdminManagementService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<CreateAdminUserResponse> CreateAdminUserAsync(CreateAdminUserRequest request)
    {
        try
        {
            var existingAdmin = await _context.AdminUsers
                .FirstOrDefaultAsync(u => u.Phone == request.Phone || u.Email == request.Email);
            
            if (existingAdmin != null)
            {
                return new CreateAdminUserResponse
                {
                    Success = false,
                    Message = "Admin user with this phone or email already exists"
                };
            }

            var adminUser = new AdminUser
            {
                Phone = request.Phone,
                Name = request.Name,
                Email = request.Email,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.AdminUsers.Add(adminUser);
            await _context.SaveChangesAsync();


            if (request.RoleIds.Any())
            {
                var roleId = request.RoleIds[0];
                var role = await _context.Roles
                    .FirstOrDefaultAsync(r => r.Id == roleId && r.IsActive);

                if (role != null)
                {
                    var adminUserRole = new AdminUserRole
                    {
                        AdminUserId = adminUser.Id,
                        RoleId = role.Id,
                        AssignedAt = DateTime.UtcNow,
                        IsActive = true
                    };
                    _context.AdminUserRoles.Add(adminUserRole);
                }
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Admin user created: {Phone} with {RoleCount} roles", 
                request.Phone, request.RoleIds.Count);

            var createdAdmin = await GetAdminUserByIdAsync(adminUser.Id);

            return new CreateAdminUserResponse
            {
                Success = true,
                Message = "Admin user created successfully",
                User = createdAdmin
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating admin user");
            return new CreateAdminUserResponse
            {
                Success = false,
                Message = "Failed to create admin user"
            };
        }
    }

    public async Task<AssignRoleResponse> AssignRoleToAdminAsync(AssignRoleRequest request)
    {
        try
        {
            var adminUser = await _context.AdminUsers.FindAsync(request.AdminUserId);
            if (adminUser == null)
            {
                return new AssignRoleResponse
                {
                    Success = false,
                    Message = "Admin user not found"
                };
            }

            var role = await _context.Roles.FindAsync(request.RoleId);
            if (role == null || !role.IsActive)
            {
                return new AssignRoleResponse
                {
                    Success = false,
                    Message = "Role not found or inactive"
                };
            }

            var existingAssignment = await _context.AdminUserRoles
                .FirstOrDefaultAsync(ur => ur.AdminUserId == request.AdminUserId);

            if (existingAssignment != null)
            {
                _context.AdminUserRoles.Remove(existingAssignment);
            }

            var adminUserRole = new AdminUserRole
            {
                AdminUserId = request.AdminUserId,
                RoleId = request.RoleId,
                AssignedAt = DateTime.UtcNow,
                IsActive = true
            };
            _context.AdminUserRoles.Add(adminUserRole);

            await _context.SaveChangesAsync();

            _logger.LogInformation("Role {RoleId} assigned to admin user {AdminUserId}", 
                request.RoleId, request.AdminUserId);

            return new AssignRoleResponse
            {
                Success = true,
                Message = "Role assigned successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning role to admin user");
            return new AssignRoleResponse
            {
                Success = false,
                Message = "Failed to assign role"
            };
        }
    }

    public async Task<bool> RemoveRoleFromAdminAsync(int adminUserId, int roleId)
    {
        try
        {
            var adminUserRole = await _context.AdminUserRoles
                .FirstOrDefaultAsync(ur => ur.AdminUserId == adminUserId && ur.RoleId == roleId);

            if (adminUserRole == null)
            {
                return false;
            }

            adminUserRole.IsActive = false;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Role {RoleId} removed from admin user {AdminUserId}", 
                roleId, adminUserId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing role from admin user");
            return false;
        }
    }

    public async Task<List<AdminUserDto>> GetAllAdminUsersAsync()
    {
        try
        {
            var adminUsers = await _context.AdminUsers
                .Include(u => u.AdminUserRole)
                    .ThenInclude(ur => ur!.Role)
                .Where(u => u.IsActive)
                .ToListAsync();

            return adminUsers.Select(MapToAdminUserDto).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all admin users");
            return new List<AdminUserDto>();
        }
    }

    public async Task<AdminUserDto?> GetAdminUserByIdAsync(int adminUserId)
    {
        try
        {
            var adminUser = await _context.AdminUsers
                .Include(u => u.AdminUserRole)
                    .ThenInclude(ur => ur!.Role)
                .FirstOrDefaultAsync(u => u.Id == adminUserId && u.IsActive);

            return adminUser != null ? MapToAdminUserDto(adminUser) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting admin user by ID");
            return null;
        }
    }

    private static AdminUserDto MapToAdminUserDto(AdminUser adminUser)
    {
        return new AdminUserDto
        {
            Id = adminUser.Id,
            Phone = adminUser.Phone,
            Name = adminUser.Name,
            Email = adminUser.Email,
            CreatedAt = adminUser.CreatedAt,
            LastLoginAt = adminUser.LastLoginAt,
            RoleName = adminUser.AdminUserRole?.IsActive == true
                      ? adminUser.AdminUserRole.Role.Name 
                      : null
        };
    }

    public async Task<List<RoleDto>> GetRolesAsync()
    {
        try
        {
            var roles = await _context.Roles
                .Include(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                .Where(r => r.IsActive)
                .ToListAsync();

            return roles.Select(MapToRoleDto).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting roles");
            return new List<RoleDto>();
        }
    }

    public async Task<List<PermissionDto>> GetPermissionsAsync()
    {
        try
        {
            var permissions = await _context.Permissions
                .Where(p => p.IsActive)
                .ToListAsync();

            return permissions.Select(MapToPermissionDto).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting permissions");
            return new List<PermissionDto>();
        }
    }

    private static RoleDto MapToRoleDto(Role role)
    {
        return new RoleDto
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            CreatedAt = role.CreatedAt,
            IsActive = role.IsActive,
            Permissions = role.RolePermissions
                .Where(rp => rp.Permission.IsActive)
                .Select(rp => MapToPermissionDto(rp.Permission))
                .ToList()
        };
    }

    private static PermissionDto MapToPermissionDto(Permission permission)
    {
        return new PermissionDto
        {
            Id = permission.Id,
            Name = permission.Name,
            Description = permission.Description,
            Resource = permission.Resource,
            Action = permission.Action,
            CreatedAt = permission.CreatedAt,
            IsActive = permission.IsActive
        };
    }
}


