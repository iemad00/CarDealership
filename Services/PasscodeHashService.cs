using System.Security.Cryptography;
using System.Text;

namespace CarDealership.Services;

public class PasscodeHashService : IPasscodeHashService
{
    private readonly IConfiguration _configuration;

    public PasscodeHashService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string HashUserPasscode(string passcode)
    {
        var salt = _configuration["PasscodeHashing:UserSalt"] ?? "user_salt_default";
        return HashPasscode(passcode, salt);
    }

    public string HashAdminPasscode(string passcode)
    {
        var salt = _configuration["PasscodeHashing:AdminSalt"] ?? "admin_salt_default";
        return HashPasscode(passcode, salt);
    }

    public bool VerifyUserPasscode(string passcode, string hash)
    {
        var hashedPasscode = HashUserPasscode(passcode);
        return hashedPasscode == hash;
    }

    public bool VerifyAdminPasscode(string passcode, string hash)
    {
        var hashedPasscode = HashAdminPasscode(passcode);
        return hashedPasscode == hash;
    }

    private static string HashPasscode(string passcode, string salt)
    {
        using var sha256 = SHA256.Create();
        var saltedPasscode = passcode + salt;
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPasscode));
        return Convert.ToBase64String(hashedBytes);
    }
}
