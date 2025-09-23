namespace CarDealership.Application.Admin.Inventory.Dtos;

public class UpdateVehicleResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public VehicleDto? Vehicle { get; set; }
}


