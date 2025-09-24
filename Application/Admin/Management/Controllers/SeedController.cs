using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CarDealership.Data.Seeds;

namespace CarDealership.Controllers.Admin;

[ApiController]
[Route("seed")]
[ApiVersion("1.0")]
[Authorize]
public class SeedController : ControllerBase
{
    private readonly DemoSeedService _demoSeedService;

    public SeedController(DemoSeedService demoSeedService)
    {
        _demoSeedService = demoSeedService;
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Seed()
    {
        await _demoSeedService.SeedAsync();
        return Ok(new { success = true, message = "Demo data seeded" });
    }
}
