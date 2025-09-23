using CarDealership.Models;

namespace CarDealership.Services;

public interface IJwtService
{
    string GenerateLoginToken(string phone);
    string GenerateAccessToken(Models.User user);
    string GenerateAccessToken(AdminUser adminUser);
    string GenerateRefreshToken(Models.User user);
    string GenerateRefreshToken(AdminUser adminUser);
    bool ValidateToken(string token);
    int? GetUserIdFromToken(string token);
    string? GetPhoneFromLoginToken(string token);
    bool ValidateRefreshToken(string token, int userId);
    Task<bool> InvalidateRefreshTokenAsync(string token);
}
