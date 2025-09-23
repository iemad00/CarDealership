using Microsoft.AspNetCore.Mvc;
using CarDealership.Models.DTOs.Auth;
using CarDealership.Services.User;

namespace CarDealership.Controllers.User;

[ApiController]
[Route("auth")]
[ApiVersion("1.0")]
public class UserAuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public UserAuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("send")]
    public async Task<IActionResult> SendOtp([FromBody] SendOtpRequest request)
    {
        var response = await _authService.SendOtpAsync(request);
        if (!response.Success)
        {
            return BadRequest(new { success = false, message = response.Message, data = new { } });
        }
        return Ok(response);
    }

    [HttpPost("verify")]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
    {
        var response = await _authService.VerifyOtpAsync(request);
        if (!response.Success)
        {
            return BadRequest(new { success = false, message = response.Message, data = new { } });
        }
        return Ok(response);
    }

    [HttpPost("authenticate")]
    public async Task<IActionResult> Authenticate([FromBody] AuthenticateRequest request)
    {
        var response = await _authService.AuthenticateAsync(request);
        if (!response.Success)
        {
            return BadRequest(new { success = false, message = response.Message, data = new { } });
        }
        return Ok(response);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var response = await _authService.RefreshTokenAsync(request);
        if (!response.Success)
        {
            return BadRequest(new { success = false, message = response.Message, data = new { } });
        }
        return Ok(response);
    }
}