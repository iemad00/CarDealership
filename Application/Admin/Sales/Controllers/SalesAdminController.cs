using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CarDealership.Services.Admin;
using CarDealership.Attributes;
using CarDealership.Application.Admin.Sales.Dtos;
using CarDealership.Application.Common.Dtos;

namespace CarDealership.Controllers.Admin;

[ApiController]
[Route("sales")]
[ApiVersion("1.0")]
[Authorize]
public class SalesAdminController : ControllerBase
{
    private readonly ISalesAdminService _service;

    public SalesAdminController(ISalesAdminService service)
    {
        _service = service;
    }

    [HttpPost("process")]
    [RequirePermission("sales", "create")]
    public async Task<IActionResult> Process([FromBody] ProcessSaleRequest request)
    {
        var resp = await _service.ProcessSaleAsync(request);
        if (!resp.Success) return BadRequest(new { success = false, message = resp.Message, data = new { } });
        return Ok(new { success = true, message = resp.Message, data = new { purchaseId = resp.Data!.PurchaseId } });
    }
}


