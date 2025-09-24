using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using CarDealership.Services.User;
using CarDealership.Application.User.Purchases.Dtos;

namespace CarDealership.Controllers.User;

[ApiController]
[Route("purchases")]
[ApiVersion("1.0")]
[Authorize]
public class UserPurchasesController : ControllerBase
{
    private readonly IUserPurchaseService _service;

    public UserPurchasesController(IUserPurchaseService service)
    {
        _service = service;
    }

    [HttpPost("request")]
    public async Task<IActionResult> RequestPurchase([FromBody] CreatePurchaseRequestRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { success = false, message = "Unauthorized", data = new { } });
        }
        var resp = await _service.CreatePurchaseRequestAsync(userId, request);
        if (!resp.Success) return BadRequest(new { success = false, message = resp.Message, data = new { } });
        return Ok(new { success = true, message = resp.Message, data = new { requestId = resp.RequestId } });
    }

    [HttpGet("history")]
    public async Task<IActionResult> History()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { success = false, message = "Unauthorized", data = new { } });
        }
        var list = await _service.GetPurchaseHistoryAsync(userId);
        return Ok(new { success = true, message = "", data = list });
    }
}


