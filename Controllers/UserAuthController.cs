using Microsoft.AspNetCore.Mvc;
using CarDealership.Models.DTOs.Auth;
using CarDealership.Services;

namespace CarDealership.Controllers;

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
        return response.Success ? Ok(response) : BadRequest(response);
    }

    [HttpPost("verify")]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
    {
        var response = await _authService.VerifyOtpAsync(request);
        return response.Success ? Ok(response) : BadRequest(response);
    }

    [HttpPost("authenticate")]
    public async Task<IActionResult> Authenticate([FromBody] AuthenticateRequest request)
    {
        var response = await _authService.AuthenticateAsync(request);
        return response.Success ? Ok(response) : BadRequest(response);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var response = await _authService.RefreshTokenAsync(request);
        return response.Success ? Ok(response) : BadRequest(response);
    }
}