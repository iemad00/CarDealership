namespace CarDealership.Application.Admin.Inventory.Dtos;

public class AddVehicleResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public VehicleDto? Vehicle { get; set; }
}


