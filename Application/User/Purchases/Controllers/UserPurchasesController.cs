using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CarDealership.Services.User;
using CarDealership.Application.User.Purchases.Dtos;
using CarDealership.Application.Common.Dtos;
using CarDealership.Attributes;

namespace CarDealership.Controllers.User;

[ApiController]
[Route("purchases")]
[ApiVersion("1.0")]
[RequireUser]
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
        var userId = (int)HttpContext.Items["UserId"]!;
        var resp = await _service.CreatePurchaseRequestAsync(userId, request);
        if (!resp.Success) return BadRequest(new { success = false, message = resp.Message, data = new { } });
        return Ok(new { success = true, message = resp.Message, data = new { requestId = resp.Data!.RequestId } });
    }

    [HttpGet("history")]
    public async Task<IActionResult> History()
    {
        var userId = (int)HttpContext.Items["UserId"]!;
        var list = await _service.GetPurchaseHistoryAsync(userId);
        return Ok(new { success = true, message = "", data = list });
    }
}


