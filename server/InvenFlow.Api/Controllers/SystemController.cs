using InvenFlow.Api.Data;
using InvenFlow.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InvenFlow.Api.Controllers;

[ApiController]
[Route("api/system")]
public class SystemController : ControllerBase
{
    readonly AppDbContext _db;
    readonly AccountingService _acct;
    public SystemController(AppDbContext db, AccountingService acct) { _db = db; _acct = acct; }

    [HttpGet("health")]
    public IActionResult Health() => Ok(new { ok = true, time = DateTime.UtcNow });

    [HttpPost("seed")]
    public async Task<IActionResult> Seed()
    {
        await SeedData.EnsureSeededAsync(_db, _acct);
        return Ok(new { ok = true });
    }

    [HttpPost("reset")]
    public async Task<IActionResult> Reset()
    {
        await _db.Database.EnsureDeletedAsync();
        await _db.Database.EnsureCreatedAsync();
        await SeedData.EnsureSeededAsync(_db, _acct);
        return Ok(new { ok = true });
    }
}
