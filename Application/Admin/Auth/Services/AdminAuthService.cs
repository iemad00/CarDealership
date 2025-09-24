using Microsoft.EntityFrameworkCore;
using CarDealership.Data;
using CarDealership.Models;
using CarDealership.Models.DTOs.Admin;
using CarDealership.Application.Common.Dtos;

namespace CarDealership.Services.Admin;

public class AdminAuthService : IAdminAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IOtpService _otpService;
    private readonly IJwtService _jwtService;
    private readonly IPasscodeHashService _passcodeHashService;
    private readonly ILogger<AdminAuthService> _logger;

    public AdminAuthService(
        ApplicationDbContext context,
        IOtpService otpService,
        IJwtService jwtService,
        IPasscodeHashService passcodeHashService,
        ILogger<AdminAuthService> logger)
    {
        _context = context;
        _otpService = otpService;
        _jwtService = jwtService;
        _passcodeHashService = passcodeHashService;
        _logger = logger;
    }

    public async Task<Response> SendOtpAsync(SendOtpRequest request)
    {
        try
        {
            var adminUser = await _context.AdminUsers
                .FirstOrDefaultAsync(u => u.Phone == request.Phone);

            if (adminUser == null)
            {
                return new Response
                {
                    Success = false,
                    Message = "Admin user not found. Please contact super admin."
                };
            }

            if (!adminUser.IsActive)
            {
                return new Response
                {
                    Success = false,
                    Message = "Admin account is deactivated. Please contact super admin."
                };
            }

            await _otpService.GenerateAndStoreOtpAsync(request.Phone);
            
            return new Response
            {
                Success = true,
                Message = "OTP sent successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending OTP to admin {Phone}", request.Phone);
            return new Response
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

            var adminUser = await _context.AdminUsers.FirstOrDefaultAsync(u => u.Phone == request.Phone);
            
            if (adminUser == null)
            {
                return new VerifyOtpResponse
                {
                    Success = false,
                    Message = "Admin user not found. Please contact super admin."
                };
            }

            if (!adminUser.IsActive)
            {
                return new VerifyOtpResponse
                {
                    Success = false,
                    Message = "Admin account is deactivated. Please contact super admin."
                };
            }

            var existingPasscode = await _context.Passcodes
                .FirstOrDefaultAsync(p => p.UserId == adminUser.Id && p.UserType == "AdminUser");
            var isFirstLogin = existingPasscode == null;

            var loginToken = _jwtService.GenerateLoginToken(request.Phone);

            return new VerifyOtpResponse
            {
                Success = true,
                Message = "OTP verified successfully",
                LoginToken = loginToken,
                FirstLogin = isFirstLogin,
                User = new AdminUserDto
                {
                    Id = adminUser.Id,
                    Phone = adminUser.Phone,
                    Name = adminUser.Name,
                    Email = adminUser.Email,
                    CreatedAt = adminUser.CreatedAt,
                    LastLoginAt = adminUser.LastLoginAt,
                    RoleName = null
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying OTP for admin {Phone}", request.Phone);
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

            var adminUser = await _context.AdminUsers
                .Include(u => u.AdminUserRole)
                    .ThenInclude(ur => ur!.Role)
                    .ThenInclude(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(u => u.Phone == phone);
            
            if (adminUser == null)
            {
                return new AuthenticateResponse
                {
                    Success = false,
                    Message = "Admin user not found"
                };
            }

            var existingPasscode = await _context.Passcodes
                .FirstOrDefaultAsync(p => p.UserId == adminUser.Id && p.UserType == "AdminUser");

            if (existingPasscode == null)
            {
                var passcodeHash = _passcodeHashService.HashAdminPasscode(request.Passcode);
                
                var passcode = new Passcode
                {
                    UserId = adminUser.Id,
                    UserType = "AdminUser",
                    Hash = passcodeHash,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    FailedAttempts = 0
                };
                
                _context.Passcodes.Add(passcode);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Passcode created for admin {Phone}", phone);
            }
            else
            {
                var isValidPasscode = VerifyPasscode(request.Passcode, existingPasscode.Hash);
                
                if (!isValidPasscode)
                {
                    existingPasscode.FailedAttempts++;
                    existingPasscode.LastAttemptAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    
                    _logger.LogWarning("Invalid passcode attempt for admin {Phone}", phone);
                    
                    return new AuthenticateResponse
                    {
                        Success = false,
                        Message = "Invalid passcode"
                    };
                }
                
                existingPasscode.FailedAttempts = 0;
                existingPasscode.LastAttemptAt = DateTime.UtcNow;
            }

            adminUser.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var accessToken = _jwtService.GenerateAccessToken(adminUser);
            var refreshToken = _jwtService.GenerateRefreshToken(adminUser);

            var roles = new List<RoleDto>();
            if (adminUser.AdminUserRole != null && 
                adminUser.AdminUserRole.IsActive)
            {
                roles.Add(new RoleDto
                {
                    Id = adminUser.AdminUserRole.Role.Id,
                    Name = adminUser.AdminUserRole.Role.Name,
                    Description = adminUser.AdminUserRole.Role.Description,
                    Permissions = adminUser.AdminUserRole.Role.RolePermissions
                        .Select(rp => new PermissionDto
                        {
                            Id = rp.Permission.Id,
                            Name = rp.Permission.Name,
                            Description = rp.Permission.Description,
                            Resource = rp.Permission.Resource,
                            Action = rp.Permission.Action
                        })
                        .ToList()
                });
            }

            return new AuthenticateResponse
            {
                Success = true,
                Message = "Authentication successful",
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                User = new AdminUserDto
                {
                    Id = adminUser.Id,
                    Phone = adminUser.Phone,
                    Name = adminUser.Name,
                    Email = adminUser.Email,
                    CreatedAt = adminUser.CreatedAt,
                    LastLoginAt = adminUser.LastLoginAt,
                    RoleName = roles.FirstOrDefault()?.Name
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during admin authentication");
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

            var adminUser = await _context.AdminUsers
                .Include(u => u.AdminUserRole)
                    .ThenInclude(ur => ur!.Role)
                    .ThenInclude(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(u => u.Id == userId.Value);
            
            if (adminUser == null)
            {
                return new RefreshTokenResponse
                {
                    Success = false,
                    Message = "Admin user not found"
                };
            }

            await _jwtService.InvalidateRefreshTokenAsync(request.RefreshToken);

            var accessToken = _jwtService.GenerateAccessToken(adminUser);
            var refreshToken = _jwtService.GenerateRefreshToken(adminUser);

            var roles = new List<RoleDto>();
            if (adminUser.AdminUserRole != null && 
                adminUser.AdminUserRole.IsActive)
            {
                roles.Add(new RoleDto
                {
                    Id = adminUser.AdminUserRole.Role.Id,
                    Name = adminUser.AdminUserRole.Role.Name,
                    Description = adminUser.AdminUserRole.Role.Description,
                    Permissions = adminUser.AdminUserRole.Role.RolePermissions
                        .Select(rp => new PermissionDto
                        {
                            Id = rp.Permission.Id,
                            Name = rp.Permission.Name,
                            Description = rp.Permission.Description,
                            Resource = rp.Permission.Resource,
                            Action = rp.Permission.Action
                        })
                        .ToList()
                });
            }

            return new RefreshTokenResponse
            {
                Success = true,
                Message = "Tokens refreshed successfully",
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                User = new AdminUserDto
                {
                    Id = adminUser.Id,
                    Phone = adminUser.Phone,
                    Name = adminUser.Name,
                    Email = adminUser.Email,
                    CreatedAt = adminUser.CreatedAt,
                    LastLoginAt = adminUser.LastLoginAt,
                    RoleName = roles.FirstOrDefault()?.Name
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing admin token");
            return new RefreshTokenResponse
            {
                Success = false,
                Message = "Failed to refresh token"
            };
        }
    }

    private bool VerifyPasscode(string passcode, string hash)
    {
        return _passcodeHashService.VerifyAdminPasscode(passcode, hash);
    }
}


