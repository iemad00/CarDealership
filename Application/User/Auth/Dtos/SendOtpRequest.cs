using System.ComponentModel.DataAnnotations;

namespace CarDealership.Models.DTOs.Auth;

public class SendOtpRequest
{
    [Required]
    [Phone]
    public string Phone { get; set; } = string.Empty;
}
