namespace CarDealership.Services;

public interface IOtpService
{
    Task<string> GenerateAndStoreOtpAsync(string phone);
    Task<OtpVerificationResult> VerifyOtpAsync(string phone, string otp);
}
