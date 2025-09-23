using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CarDealership.Services.Admin;
using CarDealership.Attributes;

namespace CarDealership.Controllers.Admin;

[ApiController]
[Route("customers")]
[ApiVersion("1.0")]
[Authorize]
public class CustomersAdminController : ControllerBase
{
    private readonly ICustomerAdminService _service;

    public CustomersAdminController(ICustomerAdminService service)
    {
        _service = service;
    }

    [HttpGet]
    [RequirePermission("customers", "read")]
    public async Task<IActionResult> GetAll()
    {
        var customers = await _service.GetAllAsync();
        return Ok(new { success = true, message = "", data = customers });
    }
}


