using CarDealership.Models.DTOs.Admin;

namespace CarDealership.Services.Admin;

public interface IAdminManagementService
{
    Task<CreateAdminUserResponse> CreateAdminUserAsync(CreateAdminUserRequest request);
    Task<AssignRoleResponse> AssignRoleToAdminAsync(AssignRoleRequest request);
    Task<bool> RemoveRoleFromAdminAsync(int adminUserId, int roleId);
    Task<List<AdminUserDto>> GetAllAdminUsersAsync();
    Task<AdminUserDto?> GetAdminUserByIdAsync(int adminUserId);
    Task<List<RoleDto>> GetRolesAsync();
    Task<List<PermissionDto>> GetPermissionsAsync();
}
