using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace CarDealership.Attributes;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class RequireUserAttribute : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        // Allow anonymous endpoints to bypass
        if (context.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any())
        {
            return;
        }

        var user = context.HttpContext.User;
        if (user?.Identity?.IsAuthenticated != true)
        {
            context.Result = new UnauthorizedObjectResult(new { success = false, message = "Unauthorized", data = new { } });
            return;
        }

        // Ensure this is an access token and extract user id
        var typeClaim = user.FindFirst("type");
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);

        if (typeClaim?.Value != "access" || userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            context.Result = new UnauthorizedObjectResult(new { success = false, message = "Unauthorized", data = new { } });
            return;
        }

        // Store for downstream usage
        context.HttpContext.Items["UserId"] = userId;
    }
}
