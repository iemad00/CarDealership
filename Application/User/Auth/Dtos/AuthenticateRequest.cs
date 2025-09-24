using System.ComponentModel.DataAnnotations;

namespace CarDealership.Models.DTOs.Auth;

public class AuthenticateRequest
{
    [Required]
    public string LoginToken { get; set; } = string.Empty;

    [Required]
    [StringLength(6, MinimumLength = 4)]
    public string Passcode { get; set; } = string.Empty;
}
