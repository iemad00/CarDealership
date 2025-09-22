using StackExchange.Redis;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace CarDealership.Services;

public class OtpService : IOtpService
{
    private readonly IDatabase _redis;
    private readonly ILogger<OtpService> _logger;

    public OtpService(IConnectionMultiplexer redis, ILogger<OtpService> logger)
    {
        _redis = redis.GetDatabase();
        _logger = logger;
    }

    public async Task<string> GenerateAndStoreOtpAsync(string phone)
    {
        // Generate 6-digit OTP
        var otp = new Random().Next(100000, 999999).ToString();
        
        // Store in Redis with 5-minute expiration
        var otpKey = $"otp:{phone}";
        await _redis.StringSetAsync(otpKey, otp, TimeSpan.FromMinutes(5));
        
        // Log OTP to console for development
        _logger.LogInformation("OTP for {Phone}: {Otp}", phone, otp);
        Console.WriteLine($"🔐 OTP for {phone}: {otp}");
        
        return otp;
    }

    public async Task<bool> VerifyOtpAsync(string phone, string otp)
    {
        var otpKey = $"otp:{phone}";
        var storedOtp = await _redis.StringGetAsync(otpKey);
        
        if (!storedOtp.HasValue)
        {
            _logger.LogWarning("No OTP found for phone: {Phone}", phone);
            return false;
        }
        
        var isValid = storedOtp == otp;
        
        if (isValid)
        {
            // Remove OTP after successful verification
            await _redis.KeyDeleteAsync(otpKey);
            _logger.LogInformation("OTP verified successfully for {Phone}", phone);
        }
        else
        {
            _logger.LogWarning("Invalid OTP for phone: {Phone}", phone);
        }
        
        return isValid;
    }
}
