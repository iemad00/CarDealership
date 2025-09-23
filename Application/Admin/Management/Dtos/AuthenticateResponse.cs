using CarDealership.Models.DTOs.Admin;

namespace CarDealership.Models.DTOs.Admin;

public class AuthenticateResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public AdminUserDto User { get; set; } = new();
}
