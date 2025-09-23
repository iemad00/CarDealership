using System.ComponentModel.DataAnnotations;

namespace CarDealership.Application.Admin.Inventory.Dtos;

public class UpdateVehicleRequest
{
    [Required]
    public int Id { get; set; }
    [Required]
    public string Make { get; set; } = string.Empty;
    [Required]
    public string Model { get; set; } = string.Empty;
    [Range(1900, 3000)]
    public int Year { get; set; }
    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }
    [Range(0, int.MaxValue)]
    public int Kilometers { get; set; }
    public string Color { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}


