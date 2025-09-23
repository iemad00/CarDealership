using System.Security.Cryptography;
using System.Text;
using CarDealership.Options;
using Microsoft.Extensions.Options;

namespace CarDealership.Services;

public class PasscodeHashService : IPasscodeHashService
{
    private readonly PasscodeHashingOptions _options;

    public PasscodeHashService(IOptions<PasscodeHashingOptions> options)
    {
        _options = options.Value;
    }

    public string HashUserPasscode(string passcode)
    {
        var salt = _options.UserSalt ?? "user_salt_default";
        return HashPasscode(passcode, salt);
    }

    public string HashAdminPasscode(string passcode)
    {
        var salt = _options.AdminSalt ?? "admin_salt_default";
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
