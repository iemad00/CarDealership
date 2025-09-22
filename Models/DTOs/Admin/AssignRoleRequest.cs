using System.ComponentModel.DataAnnotations;

namespace CarDealership.Models.DTOs.Admin;

public class AssignRoleRequest
{
    [Required]
    public int AdminUserId { get; set; }
    
    [Required]
    public int RoleId { get; set; }
    
}
