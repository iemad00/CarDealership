namespace CarDealership.Models.DTOs.Auth;

public class SendOtpResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}
