namespace CarDealership.Services.User;

public interface IVehicleBrowseService
{
    Task<List<object>> BrowseAsync(string? make, string? model, int? year, decimal? minPrice, decimal? maxPrice);
    Task<object?> GetDetailsAsync(int id);
}


