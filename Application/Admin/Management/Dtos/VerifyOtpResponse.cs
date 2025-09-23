using CarDealership.Models.DTOs.Admin;

namespace CarDealership.Models.DTOs.Admin;

public class VerifyOtpResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string LoginToken { get; set; } = string.Empty;
    public bool FirstLogin { get; set; }
    public AdminUserDto User { get; set; } = new();
}
