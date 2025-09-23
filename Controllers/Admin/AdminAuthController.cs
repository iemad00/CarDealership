using Microsoft.AspNetCore.Mvc;
using CarDealership.Models.DTOs.Admin;
using CarDealership.Services.Admin;

namespace CarDealership.Controllers.Admin;

[ApiController]
[Route("auth")]
[ApiVersion("1.0")]
public class AdminAuthController : ControllerBase
{
    private readonly IAdminAuthService _adminAuthService;

    public AdminAuthController(IAdminAuthService adminAuthService)
    {
        _adminAuthService = adminAuthService;
    }

    [HttpPost("send")]
    public async Task<IActionResult> SendOtp([FromBody] SendOtpRequest request)
    {
        var response = await _adminAuthService.SendOtpAsync(request);
        if (!response.Success)
        {
            return BadRequest(new { success = false, message = response.Message, data = new { } });
        }
        return Ok(response);
    }

    [HttpPost("verify")]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
    {
        var response = await _adminAuthService.VerifyOtpAsync(request);
        if (!response.Success)
        {
            return BadRequest(new { success = false, message = response.Message, data = new { } });
        }
        return Ok(response);
    }

    [HttpPost("authenticate")]
    public async Task<IActionResult> Authenticate([FromBody] AuthenticateRequest request)
    {
        var response = await _adminAuthService.AuthenticateAsync(request);
        if (!response.Success)
        {
            return BadRequest(new { success = false, message = response.Message, data = new { } });
        }
        return Ok(response);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var response = await _adminAuthService.RefreshTokenAsync(request);
        if (!response.Success)
        {
            return BadRequest(new { success = false, message = response.Message, data = new { } });
        }
        return Ok(response);
    }
}
