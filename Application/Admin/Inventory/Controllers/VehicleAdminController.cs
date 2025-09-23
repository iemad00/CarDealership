using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CarDealership.Application.Admin.Inventory.Dtos;
using CarDealership.Services.Admin;
using CarDealership.Attributes;

namespace CarDealership.Controllers.Admin;

[ApiController]
[Route("inventory/vehicles")]
[ApiVersion("1.0")]
[Authorize]
public class VehicleAdminController : ControllerBase
{
    private readonly IVehicleAdminService _service;

    public VehicleAdminController(IVehicleAdminService service)
    {
        _service = service;
    }

    [HttpPost]
    [RequirePermission("vehicles", "create")]
    public async Task<IActionResult> Add([FromBody] AddVehicleRequest request)
    {
        var response = await _service.AddVehicleAsync(request);
        if (!response.Success)
        {
            return BadRequest(new { success = false, message = response.Message, data = new { } });
        }
        return Ok(new { success = true, message = response.Message, data = response.Vehicle });
    }

    [HttpPatch("{id}")]
    [RequirePermission("vehicles", "update")]
    public async Task<IActionResult> Patch(int id, [FromBody] PatchVehicleRequest request)
    {
        var response = await _service.PatchVehicleAsync(id, request);
        if (!response.Success)
        {
            return BadRequest(new { success = false, message = response.Message, data = new { } });
        }
        return Ok(new { success = true, message = response.Message, data = response.Vehicle });
    }
}


