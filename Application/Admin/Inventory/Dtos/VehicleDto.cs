namespace CarDealership.Application.Admin.Inventory.Dtos;

public class VehicleDto
{
    public int Id { get; set; }
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public decimal Price { get; set; }
    public int Kilometers { get; set; }
    public string Color { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}


