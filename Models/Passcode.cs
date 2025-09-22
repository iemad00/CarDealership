namespace CarDealership.Models;

public class Passcode
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Hash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;
    public int FailedAttempts { get; set; } = 0;
    public DateTime? LastAttemptAt { get; set; }
    public User User { get; set; } = null!;
}
