using CarDealership.Models.DTOs.User;

namespace CarDealership.Models.DTOs.Auth;

public class VerifyOtpResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string LoginToken { get; set; } = string.Empty;
    public bool FirstLogin { get; set; }
    public UserDto User { get; set; } = new();
}
