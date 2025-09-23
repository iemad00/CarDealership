using StackExchange.Redis;
using System.Security.Cryptography;
using System.Text;

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
        // Generate 6-digit OTP using cryptographically secure RNG
        var otp = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
        
        // Store hashed OTP and counters in Redis with 5-minute expiration
        var otpKey = $"otp:{phone}";
        // Ensure key is removed to avoid WRONGTYPE if it exists as a string from older versions
        await _redis.KeyDeleteAsync(otpKey);
        var otpHashBase64 = ComputeOtpHashBase64(otp);
        await _redis.HashSetAsync(otpKey, new HashEntry[]
        {
            new HashEntry("hash", otpHashBase64),
            new HashEntry("attempts", 0)
        });
        await _redis.KeyExpireAsync(otpKey, TimeSpan.FromMinutes(5));
        
        // Log OTP only in Development
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? string.Empty;
        if (env.Equals("Development", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogInformation("OTP for {Phone}: {Otp}", phone, otp);
            Console.WriteLine($"🔐 OTP for {phone}: {otp}");
        }
        
        return otp;
    }

    public async Task<OtpVerificationResult> VerifyOtpAsync(string phone, string otp)
    {
        var otpKey = $"otp:{phone}";

        // Check existence
        if (!await _redis.KeyExistsAsync(otpKey))
        {
            _logger.LogWarning("No OTP found for phone: {Phone}", phone);
            return new OtpVerificationResult { Success = false, ErrorCode = OtpErrorCode.NotFound, Message = "OTP not found or expired" };
        }

        // Backward compatibility: handle both string and hash types
        var keyType = await _redis.KeyTypeAsync(otpKey);
        if (keyType == RedisType.String)
        {
            // Old behavior: OTP stored as plain string
            var storedOtp = await _redis.StringGetAsync(otpKey);
            if (!storedOtp.HasValue)
            {
                _logger.LogWarning("No OTP found for phone: {Phone}", phone);
                return new OtpVerificationResult { Success = false, ErrorCode = OtpErrorCode.NotFound, Message = "OTP not found or expired" };
            }

            var isValidLegacy = storedOtp == otp;
            if (isValidLegacy)
            {
                await _redis.KeyDeleteAsync(otpKey);
                _logger.LogInformation("OTP verified successfully for {Phone}", phone);
                return new OtpVerificationResult { Success = true };
            }

            _logger.LogWarning("Invalid OTP for phone: {Phone}", phone);
            return new OtpVerificationResult { Success = false, ErrorCode = OtpErrorCode.Invalid, Message = "Invalid OTP" };
        }

        // Check lockout
        var lockedUntilValue = await _redis.HashGetAsync(otpKey, "lockedUntil");
        if (lockedUntilValue.HasValue && long.TryParse(lockedUntilValue!, out var ticks))
        {
            var lockedUntil = new DateTime(ticks, DateTimeKind.Utc);
            if (DateTime.UtcNow < lockedUntil)
            {
                _logger.LogWarning("OTP attempts locked for phone: {Phone}", phone);
                return new OtpVerificationResult { Success = false, ErrorCode = OtpErrorCode.Locked, Message = "Too many attempts. Please try again later." };
            }
        }

        // Compare hashes in constant time
        var storedHash = await _redis.HashGetAsync(otpKey, "hash");
        if (!storedHash.HasValue)
        {
            _logger.LogWarning("OTP data incomplete for phone: {Phone}", phone);
            return new OtpVerificationResult { Success = false, ErrorCode = OtpErrorCode.NotFound, Message = "OTP not found or expired" };
        }

        var providedHashBase64 = ComputeOtpHashBase64(otp);
        var isValid = ConstantTimeEquals(storedHash!, providedHashBase64);

        if (isValid)
        {
            await _redis.KeyDeleteAsync(otpKey);
            _logger.LogInformation("OTP verified successfully for {Phone}", phone);
            return new OtpVerificationResult { Success = true };
        }

        // Increment attempts and possibly lock
        var attempts = await _redis.HashIncrementAsync(otpKey, "attempts", 1);
        if (attempts >= 5)
        {
            var lockoutUntil = DateTime.UtcNow.AddMinutes(2);
            await _redis.HashSetAsync(otpKey, new HashEntry[] { new HashEntry("lockedUntil", lockoutUntil.Ticks) });
            _logger.LogWarning("OTP locked due to too many attempts for {Phone}", phone);
        }
        else
        {
            _logger.LogWarning("Invalid OTP for phone: {Phone}. Attempts: {Attempts}", phone, attempts);
        }

        return new OtpVerificationResult { Success = false, ErrorCode = OtpErrorCode.Invalid, Message = "Invalid OTP" };
    }

    private static string ComputeOtpHashBase64(string otp)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(otp);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }

    private static bool ConstantTimeEquals(string a, string b)
    {
        var aBytes = Encoding.UTF8.GetBytes(a);
        var bBytes = Encoding.UTF8.GetBytes(b);
        return aBytes.Length == bBytes.Length && CryptographicOperations.FixedTimeEquals(aBytes, bBytes);
    }
}
