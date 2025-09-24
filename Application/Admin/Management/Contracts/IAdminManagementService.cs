using CarDealership.Models.DTOs.Admin;
using CarDealership.Application.Common.Dtos;

namespace CarDealership.Services.Admin;

public interface IAdminManagementService
{
    Task<Response<AdminUserDto>> CreateAdminUserAsync(CreateAdminUserRequest request);
    Task<Response> AssignRoleToAdminAsync(AssignRoleRequest request);
    Task<bool> RemoveRoleFromAdminAsync(int adminUserId, int roleId);
    Task<List<AdminUserDto>> GetAllAdminUsersAsync();
    Task<AdminUserDto?> GetAdminUserByIdAsync(int adminUserId);
    Task<List<RoleDto>> GetRolesAsync();
    Task<List<PermissionDto>> GetPermissionsAsync();
}
