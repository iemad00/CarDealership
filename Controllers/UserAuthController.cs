using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CarDealership.Data;
using CarDealership.Models;
using CarDealership.Models.DTOs.Auth;
using CarDealership.Models.DTOs.User;
using CarDealership.Services;
using System.Security.Cryptography;
using System.Text;

namespace CarDealership.Controllers;

[ApiController]
[Route("auth")]
[ApiVersion("1.0")]
public class UserAuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IOtpService _otpService;
    private readonly IJwtService _jwtService;
    private readonly ILogger<UserAuthController> _logger;

    public UserAuthController(
        ApplicationDbContext context,
        IOtpService otpService,
        IJwtService jwtService,
        ILogger<UserAuthController> logger)
    {
        _context = context;
        _otpService = otpService;
        _jwtService = jwtService;
        _logger = logger;
    }

    [HttpPost("send")]
    public async Task<IActionResult> SendOtp([FromBody] SendOtpRequest request)
    {
        try
        {
            // Generate and store OTP
            await _otpService.GenerateAndStoreOtpAsync(request.Phone);
            
            return Ok(new SendOtpResponse
            {
                Success = true,
                Message = "OTP sent successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending OTP for {Phone}", request.Phone);
            return BadRequest(new SendOtpResponse
            {
                Success = false,
                Message = "Failed to send OTP"
            });
        }
    }

    [HttpPost("verify")]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
    {
        try
        {
            // Verify OTP
            var isValidOtp = await _otpService.VerifyOtpAsync(request.Phone, request.Otp);
            
            if (!isValidOtp)
            {
                return BadRequest(new VerifyOtpResponse
                {
                    Success = false,
                    Message = "Invalid OTP"
                });
            }

            // Check if user exists
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Phone == request.Phone);
            var isFirstLogin = user == null;

            // Create user if doesn't exist
            if (user == null)
            {
                user = new User
                {
                    Phone = request.Phone,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };
                
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("New user created: {Phone}", request.Phone);
            }

            // Generate login token (JWT)
            var loginToken = _jwtService.GenerateLoginToken(request.Phone);

            return Ok(new AuthResponse
            {
                Success = true,
                Message = "OTP verified successfully",
                LoginToken = loginToken,
                FirstLogin = isFirstLogin,
                User = new UserDto
                {
                    Id = user.Id,
                    Phone = user.Phone,
                    CreatedAt = user.CreatedAt
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying OTP for {Phone}", request.Phone);
            return BadRequest(new AuthResponse
            {
                Success = false,
                Message = "Failed to verify OTP"
            });
        }
    }

    [HttpPost("authenticate")]
    public async Task<IActionResult> Authenticate([FromBody] AuthenticateRequest request)
    {
        try
        {
            // Get phone number from login token (JWT)
            var phoneNumber = _jwtService.GetPhoneFromLoginToken(request.LoginToken);
            
            if (string.IsNullOrEmpty(phoneNumber))
            {
                return BadRequest(new AuthResponse
                {
                    Success = false,
                    Message = "Invalid or expired login token"
                });
            }

            // Get user
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Phone == phoneNumber);
            
            if (user == null)
            {
                return BadRequest(new AuthResponse
                {
                    Success = false,
                    Message = "User not found"
                });
            }

            // Check if this is first login (no passcode set)
            var existingPasscode = await _context.Passcodes.FirstOrDefaultAsync(p => p.UserId == user.Id);
            
            if (existingPasscode == null)
            {
                // First login - create passcode
                var passcodeHash = HashPasscode(request.Passcode);
                
                var passcode = new Passcode
                {
                    UserId = user.Id,
                    Hash = passcodeHash,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };
                
                _context.Passcodes.Add(passcode);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Passcode set for user: {Phone}", phoneNumber);
            }
            else
            {
                // Verify existing passcode
                if (!VerifyPasscode(request.Passcode, existingPasscode.Hash))
                {
                    return BadRequest(new AuthResponse
                    {
                        Success = false,
                        Message = "Invalid passcode"
                    });
                }
            }

            // Generate tokens
            var accessToken = _jwtService.GenerateAccessToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken(user);

            return Ok(new AuthResponse
            {
                Success = true,
                Message = "Authentication successful",
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                User = new UserDto
                {
                    Id = user.Id,
                    Phone = user.Phone,
                    CreatedAt = user.CreatedAt
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during authentication");
            return BadRequest(new AuthResponse
            {
                Success = false,
                Message = "Authentication failed"
            });
        }
    }

    private string HashPasscode(string passcode)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(passcode));
        return Convert.ToBase64String(hashedBytes);
    }

    private bool VerifyPasscode(string passcode, string hash)
    {
        var hashedPasscode = HashPasscode(passcode);
        return hashedPasscode == hash;
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        try
        {
            // Get user ID from refresh token
            var userId = _jwtService.GetUserIdFromToken(request.RefreshToken);
            
            if (userId == null)
            {
                return BadRequest(new AuthResponse
                {
                    Success = false,
                    Message = "Invalid refresh token"
                });
            }

            // Validate refresh token
            if (!_jwtService.ValidateRefreshToken(request.RefreshToken, userId.Value))
            {
                return BadRequest(new AuthResponse
                {
                    Success = false,
                    Message = "Invalid or expired refresh token"
                });
            }

            // Get user
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId.Value);
            
            if (user == null)
            {
                return BadRequest(new AuthResponse
                {
                    Success = false,
                    Message = "User not found"
                });
            }

            // Invalidate old refresh token
            await _jwtService.InvalidateRefreshTokenAsync(request.RefreshToken);

            // Generate new tokens
            var accessToken = _jwtService.GenerateAccessToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken(user);

            return Ok(new AuthResponse
            {
                Success = true,
                Message = "Tokens refreshed successfully",
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                User = new UserDto
                {
                    Id = user.Id,
                    Phone = user.Phone,
                    CreatedAt = user.CreatedAt
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token");
            return BadRequest(new AuthResponse
            {
                Success = false,
                Message = "Failed to refresh token"
            });
        }
    }
}
