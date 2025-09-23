using System.ComponentModel.DataAnnotations;

namespace CarDealership.Models.DTOs.Admin;

public class CreateAdminUserRequest
{
    [Required]
    [Phone]
    public string Phone { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    public List<int> RoleIds { get; set; } = new();
}
