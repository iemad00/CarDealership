using Microsoft.EntityFrameworkCore;
using CarDealership.Data;

namespace CarDealership.Services.User;

public class VehicleBrowseService : IVehicleBrowseService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<VehicleBrowseService> _logger;

    public VehicleBrowseService(ApplicationDbContext context, ILogger<VehicleBrowseService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<object>> BrowseAsync(string? make, string? model, int? year, decimal? minPrice, decimal? maxPrice)
    {
        var query = _context.Vehicles.AsNoTracking().Where(v => v.IsActive);
        if (!string.IsNullOrWhiteSpace(make)) query = query.Where(v => v.Make == make);
        if (!string.IsNullOrWhiteSpace(model)) query = query.Where(v => v.Model == model);
        if (year.HasValue) query = query.Where(v => v.Year == year.Value);
        if (minPrice.HasValue) query = query.Where(v => v.Price >= minPrice.Value);
        if (maxPrice.HasValue) query = query.Where(v => v.Price <= maxPrice.Value);

        return await query
            .OrderByDescending(v => v.CreatedAt)
            .Select(v => new { v.Id, v.Make, v.Model, v.Year, v.Price, v.Kilometers, v.Color })
            .ToListAsync<object>();
    }

    public async Task<object?> GetDetailsAsync(int id)
    {
        return await _context.Vehicles
            .AsNoTracking()
            .Where(x => x.Id == id && x.IsActive)
            .Select(v => new
            {
                v.Id,
                v.Make,
                v.Model,
                v.Year,
                v.Price,
                v.Kilometers,
                v.Color,
                v.Description
            })
            .FirstOrDefaultAsync<object>();
    }
}


