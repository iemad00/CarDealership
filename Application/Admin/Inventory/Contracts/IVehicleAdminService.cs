using CarDealership.Application.Admin.Inventory.Dtos;

namespace CarDealership.Services.Admin;

public interface IVehicleAdminService
{
    Task<AddVehicleResponse> AddVehicleAsync(AddVehicleRequest request);
    Task<UpdateVehicleResponse> UpdateVehicleAsync(UpdateVehicleRequest request);
    Task<UpdateVehicleResponse> PatchVehicleAsync(int id, PatchVehicleRequest request);
}


