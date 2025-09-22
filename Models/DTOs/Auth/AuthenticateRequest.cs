namespace CarDealership.Models.DTOs.Auth;

public class AuthenticateRequest
{
    public string LoginToken { get; set; } = string.Empty;
    public string Passcode { get; set; } = string.Empty;
}
