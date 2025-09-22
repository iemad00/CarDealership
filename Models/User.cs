namespace CarDealership.Models;

public class User
{
    public int Id { get; set; }
    public string Phone { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    // Passcode relationship handled separately
}
