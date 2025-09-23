namespace CarDealership.Application.Admin.Inventory.Dtos;

public class PatchVehicleRequest
{
    public string? Make { get; set; }
    public string? Model { get; set; }
    public int? Year { get; set; }
    public decimal? Price { get; set; }
    public int? Kilometers { get; set; }
    public string? Color { get; set; }
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
}


