using CarDealership.Models;

namespace CarDealership.Services;

public interface IJwtService
{
    string GenerateLoginToken(string phone);
    string GenerateAccessToken(User user);
    string GenerateRefreshToken(User user);
    bool ValidateToken(string token);
    int? GetUserIdFromToken(string token);
    string? GetPhoneFromLoginToken(string token);
    bool ValidateRefreshToken(string token, int userId);
    Task<bool> InvalidateRefreshTokenAsync(string token);
}
