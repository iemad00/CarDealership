using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using CarDealership.Services;

namespace CarDealership.Attributes;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class RequirePermissionAttribute : Attribute, IAuthorizationFilter
{
    private readonly string _resource;
    private readonly string _action;

    public RequirePermissionAttribute(string resource, string action)
    {
        _resource = resource;
        _action = action;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        // Skip authorization if action is marked with [AllowAnonymous]
        if (context.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any())
        {
            return;
        }

        // Get user ID from JWT token
        var userIdClaim = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            context.Result = new UnauthorizedObjectResult(new { Message = "User not authenticated" });
            return;
        }

        // Get user type from JWT token
        var userTypeClaim = context.HttpContext.User.FindFirst("userType");
        if (userTypeClaim?.Value != "AdminUser")
        {
            context.Result = new ForbidResult();
            return;
        }

        // Get admin user with roles and permissions from database
        var adminUser = GetAdminUserWithPermissions(context.HttpContext, userId).Result;
        if (adminUser == null)
        {
            context.Result = new UnauthorizedObjectResult(new { Message = "Admin user not found" });
            return;
        }

        // Check if admin user has the required permission (single role)
        var hasPermission = adminUser.AdminUserRole != null &&
                           adminUser.AdminUserRole.IsActive &&
                           adminUser.AdminUserRole.Role.RolePermissions
                               .Any(rp => rp.Permission.Resource.Equals(_resource, StringComparison.OrdinalIgnoreCase) &&
                                         rp.Permission.Action.Equals(_action, StringComparison.OrdinalIgnoreCase));

        if (!hasPermission)
        {
            context.Result = new ForbidResult();
        }
    }

    private static async Task<Models.AdminUser?> GetAdminUserWithPermissions(HttpContext context, int userId)
    {
        var dbContext = context.RequestServices.GetRequiredService<Data.ApplicationDbContext>();
        return await dbContext.AdminUsers
            .Include(u => u.AdminUserRole)
                .ThenInclude(ur => ur!.Role)
                .ThenInclude(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);
    }
}
