using System.ComponentModel.DataAnnotations;

namespace CarDealership.Options;

public class PasscodeHashingOptions
{
    public const string SectionName = "PasscodeHashing";

    [Required]
    public string UserSalt { get; set; } = string.Empty;

    [Required]
    public string AdminSalt { get; set; } = string.Empty;
}


