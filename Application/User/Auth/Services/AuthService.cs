using Microsoft.EntityFrameworkCore;
using CarDealership.Data;
using CarDealership.Models;
using CarDealership.Models.DTOs.Auth;
using CarDealership.Models.DTOs.User;

namespace CarDealership.Services.User;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IOtpService _otpService;
    private readonly IJwtService _jwtService;
    private readonly IPasscodeHashService _passcodeHashService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        ApplicationDbContext context,
        IOtpService otpService,
        IJwtService jwtService,
        IPasscodeHashService passcodeHashService,
        ILogger<AuthService> logger)
    {
        _context = context;
        _otpService = otpService;
        _jwtService = jwtService;
        _passcodeHashService = passcodeHashService;
        _logger = logger;
    }

    public async Task<SendOtpResponse> SendOtpAsync(SendOtpRequest request)
    {
        try
        {
            await _otpService.GenerateAndStoreOtpAsync(request.Phone);
            
            return new SendOtpResponse
            {
                Success = true,
                Message = "OTP sent successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending OTP to {Phone}", request.Phone);
            return new SendOtpResponse
            {
                Success = false,
                Message = "Failed to send OTP"
            };
        }
    }

    public async Task<VerifyOtpResponse> VerifyOtpAsync(VerifyOtpRequest request)
    {
        try
        {
            var otpResult = await _otpService.VerifyOtpAsync(request.Phone, request.Otp);
            if (!otpResult.Success)
            {
                return new VerifyOtpResponse
                {
                    Success = false,
                    Message = string.IsNullOrWhiteSpace(otpResult.Message) ? "Invalid OTP" : otpResult.Message
                };
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Phone == request.Phone);
            var isFirstLogin = user == null;

            if (user == null)
            {
                user = new Models.User
                {
                    Phone = request.Phone,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };
                
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                _logger.LogInformation("New user created with phone: {Phone}", request.Phone);
            }

            var loginToken = _jwtService.GenerateLoginToken(request.Phone);

            return new VerifyOtpResponse
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
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying OTP for {Phone}", request.Phone);
            return new VerifyOtpResponse
            {
                Success = false,
                Message = "Failed to verify OTP"
            };
        }
    }

    public async Task<AuthenticateResponse> AuthenticateAsync(AuthenticateRequest request)
    {
        try
        {
            var phone = _jwtService.GetPhoneFromLoginToken(request.LoginToken);
            
            if (string.IsNullOrEmpty(phone))
            {
                return new AuthenticateResponse
                {
                    Success = false,
                    Message = "Invalid login token"
                };
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Phone == phone);
            
            if (user == null)
            {
                return new AuthenticateResponse
                {
                    Success = false,
                    Message = "User not found"
                };
            }

            var existingPasscode = await _context.Passcodes
                .FirstOrDefaultAsync(p => p.UserId == user.Id && p.UserType == "User");

            if (existingPasscode == null)
            {
                var passcodeHash = _passcodeHashService.HashUserPasscode(request.Passcode);
                
                var passcode = new Passcode
                {
                    UserId = user.Id,
                    UserType = "User",
                    Hash = passcodeHash,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    FailedAttempts = 0
                };
                
                _context.Passcodes.Add(passcode);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Passcode created for user {Phone}", phone);
            }
            else
            {
                var isValidPasscode = VerifyPasscode(request.Passcode, existingPasscode.Hash);
                
                if (!isValidPasscode)
                {
                    existingPasscode.FailedAttempts++;
                    existingPasscode.LastAttemptAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    
                    _logger.LogWarning("Invalid passcode attempt for user {Phone}", phone);
                    
                    return new AuthenticateResponse
                    {
                        Success = false,
                        Message = "Invalid passcode"
                    };
                }
                
                existingPasscode.FailedAttempts = 0;
                existingPasscode.LastAttemptAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            var accessToken = _jwtService.GenerateAccessToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken(user);

            return new AuthenticateResponse
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
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during authentication");
            return new AuthenticateResponse
            {
                Success = false,
                Message = "Authentication failed"
            };
        }
    }

    public async Task<RefreshTokenResponse> RefreshTokenAsync(RefreshTokenRequest request)
    {
        try
        {
            var userId = _jwtService.GetUserIdFromToken(request.RefreshToken);
            
            if (userId == null)
            {
                return new RefreshTokenResponse
                {
                    Success = false,
                    Message = "Invalid refresh token"
                };
            }

            if (!_jwtService.ValidateRefreshToken(request.RefreshToken, userId.Value))
            {
                return new RefreshTokenResponse
                {
                    Success = false,
                    Message = "Invalid or expired refresh token"
                };
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId.Value);
            
            if (user == null)
            {
                return new RefreshTokenResponse
                {
                    Success = false,
                    Message = "User not found"
                };
            }

            await _jwtService.InvalidateRefreshTokenAsync(request.RefreshToken);

            var accessToken = _jwtService.GenerateAccessToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken(user);

            return new RefreshTokenResponse
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
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token");
            return new RefreshTokenResponse
            {
                Success = false,
                Message = "Failed to refresh token"
            };
        }
    }

    private bool VerifyPasscode(string passcode, string hash)
    {
        return _passcodeHashService.VerifyUserPasscode(passcode, hash);
    }
}


