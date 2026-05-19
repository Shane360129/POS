using InvenFlow.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace InvenFlow.Api.Controllers;

[ApiController]
[Route("api/reports")]
public class ReportsController : ControllerBase
{
    readonly ReportService _svc;
    public ReportsController(ReportService svc) { _svc = svc; }

    [HttpGet("dashboard")] public Task<object> Dashboard() => _svc.DashboardAsync();
    [HttpGet("gross-profit")] public Task<object> GrossProfit([FromQuery] DateTime? from, [FromQuery] DateTime? to)
        => _svc.GrossProfitAsync(from ?? DateTime.UtcNow.AddDays(-90), to ?? DateTime.UtcNow);
    [HttpGet("product-ranking")] public Task<object> Ranking([FromQuery] int top = 10) => _svc.ProductRankingAsync(top);
    [HttpGet("inventory-turnover")] public Task<object> Turnover() => _svc.InventoryTurnoverAsync();
    [HttpGet("trial-balance")] public Task<object> TrialBalance([FromQuery] DateTime? from, [FromQuery] DateTime? to)
        => _svc.TrialBalanceAsync(from ?? DateTime.UtcNow.AddDays(-90), to ?? DateTime.UtcNow);
}
