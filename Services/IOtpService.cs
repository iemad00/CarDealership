namespace CarDealership.Services;

public interface IOtpService
{
    Task<string> GenerateAndStoreOtpAsync(string phone);
    Task<bool> VerifyOtpAsync(string phone, string otp);
}
