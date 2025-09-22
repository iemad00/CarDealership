namespace CarDealership.Models;

public class RolePermission
{
    public int Id { get; set; }
    public int RoleId { get; set; }
    public int PermissionId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public Role Role { get; set; } = default!;
    public Permission Permission { get; set; } = default!;
}
