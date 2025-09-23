namespace CarDealership.Models;

public class AdminUserRole
{
    public int Id { get; set; }
    public int AdminUserId { get; set; }
    public int RoleId { get; set; }
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public AdminUser AdminUser { get; set; } = default!;
    public Role Role { get; set; } = default!;
}
