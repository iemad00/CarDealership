using Microsoft.EntityFrameworkCore;
using CarDealership.Data;
using CarDealership.Application.Admin.Inventory.Dtos;
using CarDealership.Application.Common.Dtos;

namespace CarDealership.Services.Admin;

public class VehicleAdminService : IVehicleAdminService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<VehicleAdminService> _logger;

    public VehicleAdminService(ApplicationDbContext context, ILogger<VehicleAdminService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Response<VehicleDto>> AddVehicleAsync(AddVehicleRequest request)
    {
        try
        {
            var vehicle = new Models.Vehicle
            {
                Make = request.Make,
                Model = request.Model,
                Year = request.Year,
                Price = request.Price,
                Kilometers = request.Kilometers,
                Color = request.Color,
                Description = request.Description,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Vehicles.Add(vehicle);
            await _context.SaveChangesAsync();

            return new Response<VehicleDto>
            {
                Success = true,
                Message = "Vehicle added",
                Data = Map(vehicle)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding vehicle");
            return new Response<VehicleDto> { Success = false, Message = "Failed to add vehicle" };
        }
    }

    public async Task<Response<VehicleDto>> UpdateVehicleAsync(UpdateVehicleRequest request)
    {
        try
        {
            var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == request.Id);
            if (vehicle == null)
            {
                return new Response<VehicleDto> { Success = false, Message = "Vehicle not found" };
            }

            vehicle.Make = request.Make;
            vehicle.Model = request.Model;
            vehicle.Year = request.Year;
            vehicle.Price = request.Price;
            vehicle.Kilometers = request.Kilometers;
            vehicle.Color = request.Color;
            vehicle.Description = request.Description;
            vehicle.IsActive = request.IsActive;

            await _context.SaveChangesAsync();

            return new Response<VehicleDto>
            {
                Success = true,
                Message = "Vehicle updated",
                Data = Map(vehicle)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating vehicle {Id}", request.Id);
            return new Response<VehicleDto> { Success = false, Message = "Failed to update vehicle" };
        }
    }

    public async Task<Response<VehicleDto>> PatchVehicleAsync(int id, PatchVehicleRequest request)
    {
        try
        {
            var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == id);
            if (vehicle == null)
            {
                return new Response<VehicleDto> { Success = false, Message = "Vehicle not found" };
            }

            if (request.Make != null) vehicle.Make = request.Make;
            if (request.Model != null) vehicle.Model = request.Model;
            if (request.Year.HasValue) vehicle.Year = request.Year.Value;
            if (request.Price.HasValue) vehicle.Price = request.Price.Value;
            if (request.Kilometers.HasValue) vehicle.Kilometers = request.Kilometers.Value;
            if (request.Color != null) vehicle.Color = request.Color;
            if (request.Description != null) vehicle.Description = request.Description;
            if (request.IsActive.HasValue) vehicle.IsActive = request.IsActive.Value;

            await _context.SaveChangesAsync();

            return new Response<VehicleDto>
            {
                Success = true,
                Message = "Vehicle updated",
                Data = Map(vehicle)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error patching vehicle {Id}", id);
            return new Response<VehicleDto> { Success = false, Message = "Failed to update vehicle" };
        }
    }

    private static VehicleDto Map(Models.Vehicle v)
    {
        return new VehicleDto
        {
            Id = v.Id,
            Make = v.Make,
            Model = v.Model,
            Year = v.Year,
            Price = v.Price,
            Kilometers = v.Kilometers,
            Color = v.Color,
            Description = v.Description,
            IsActive = v.IsActive
        };
    }
}


