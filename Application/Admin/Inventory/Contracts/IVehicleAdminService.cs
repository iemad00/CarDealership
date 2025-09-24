using CarDealership.Application.Admin.Inventory.Dtos;
using CarDealership.Application.Common.Dtos;

namespace CarDealership.Services.Admin;

public interface IVehicleAdminService
{
    Task<Response<VehicleDto>> AddVehicleAsync(AddVehicleRequest request);
    Task<Response<VehicleDto>> UpdateVehicleAsync(UpdateVehicleRequest request);
    Task<Response<VehicleDto>> PatchVehicleAsync(int id, PatchVehicleRequest request);
}


