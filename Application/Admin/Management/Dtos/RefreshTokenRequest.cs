using System.ComponentModel.DataAnnotations;

namespace CarDealership.Models.DTOs.Admin;

public class RefreshTokenRequest
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}
