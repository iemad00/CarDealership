using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CarDealership.Models.DTOs.Admin;
using CarDealership.Services.Admin;
using CarDealership.Attributes;

namespace CarDealership.Controllers.Admin;

[ApiController]
[Route("admin-management")]
[ApiVersion("1.0")]
[Authorize]
public class AdminManagementController : ControllerBase
{
    private readonly IAdminManagementService _adminManagementService;

    public AdminManagementController(IAdminManagementService adminManagementService)
    {
        _adminManagementService = adminManagementService;
    }

    [HttpPost("create-admin")]
    [RequirePermission("admin_users", "create")]
    public async Task<IActionResult> CreateAdminUser([FromBody] CreateAdminUserRequest request)
    {
        var response = await _adminManagementService.CreateAdminUserAsync(request);
        return response.Success ? Ok(response) : BadRequest(response);
    }

    [HttpPost("assign-role")]
    [RequirePermission("admin_users", "update")]
    public async Task<IActionResult> AssignRole([FromBody] AssignRoleRequest request)
    {
        var response = await _adminManagementService.AssignRoleToAdminAsync(request);
        return response.Success ? Ok(response) : BadRequest(response);
    }

    [HttpDelete("remove-role/{adminUserId}/{roleId}")]
    [RequirePermission("admin_users", "update")]
    public async Task<IActionResult> RemoveRole(int adminUserId, int roleId)
    {
        var success = await _adminManagementService.RemoveRoleFromAdminAsync(adminUserId, roleId);
        return success ? Ok(new { Success = true, Message = "Role removed successfully" }) 
                      : BadRequest(new { Success = false, Message = "Failed to remove role" });
    }

    [HttpGet("admin-users")]
    [RequirePermission("admin_users", "read")]
    public async Task<IActionResult> GetAllAdminUsers()
    {
        var adminUsers = await _adminManagementService.GetAllAdminUsersAsync();
        return Ok(adminUsers);
    }

    [HttpGet("admin-users/{adminUserId}")]
    [RequirePermission("admin_users", "read")]
    public async Task<IActionResult> GetAdminUser(int adminUserId)
    {
        var adminUser = await _adminManagementService.GetAdminUserByIdAsync(adminUserId);
        return adminUser != null ? Ok(adminUser) : NotFound(new { Message = "Admin user not found" });
    }

    [HttpGet("roles")]
    [RequirePermission("roles", "read")]
    public async Task<IActionResult> GetRoles()
    {
        var roles = await _adminManagementService.GetRolesAsync();
        return Ok(roles);
    }

    [HttpGet("permissions")]
    [RequirePermission("permissions", "read")]
    public async Task<IActionResult> GetPermissions()
    {
        var permissions = await _adminManagementService.GetPermissionsAsync();
        return Ok(permissions);
    }
}
