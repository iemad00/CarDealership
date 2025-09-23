namespace CarDealership.Models;

public class Vehicle
{
    public int Id { get; set; }
    public string Make { get; set; } = string.Empty; // e.g., Toyota
    public string Model { get; set; } = string.Empty; // e.g., Corolla
    public int Year { get; set; }
    public decimal Price { get; set; }
    public int Kilometers { get; set; }
    public string Color { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
}


