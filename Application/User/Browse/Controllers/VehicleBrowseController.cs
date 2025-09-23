using Microsoft.AspNetCore.Mvc;
using CarDealership.Services.User;

namespace CarDealership.Controllers.User;

[ApiController]
[Route("vehicles")]
[ApiVersion("1.0")]
public class VehicleBrowseController : ControllerBase
{
    private readonly IVehicleBrowseService _service;

    public VehicleBrowseController(IVehicleBrowseService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Browse([FromQuery] string? make, [FromQuery] string? model, [FromQuery] int? year, [FromQuery] decimal? minPrice, [FromQuery] decimal? maxPrice)
    {
        var results = await _service.BrowseAsync(make, model, year, minPrice, maxPrice);

        return Ok(new { success = true, message = "", data = results });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Details(int id)
    {
        var v = await _service.GetDetailsAsync(id);
        if (v == null)
        {
            return NotFound(new { success = false, message = "Vehicle not found", data = new { } });
        }
        return Ok(new { success = true, message = "", data = v });
    }
}


