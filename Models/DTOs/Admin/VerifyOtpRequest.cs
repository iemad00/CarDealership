using System.ComponentModel.DataAnnotations;

namespace CarDealership.Models.DTOs.Admin;

public class VerifyOtpRequest
{
    [Required]
    [Phone]
    public string Phone { get; set; } = string.Empty;
    
    [Required]
    [StringLength(6, MinimumLength = 6)]
    public string Otp { get; set; } = string.Empty;
}
