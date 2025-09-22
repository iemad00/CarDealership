namespace CarDealership.Services;

public interface IPasscodeHashService
{
    string HashUserPasscode(string passcode);
    string HashAdminPasscode(string passcode);
    bool VerifyUserPasscode(string passcode, string hash);
    bool VerifyAdminPasscode(string passcode, string hash);
}
