using FitMetrics.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FitMetrics.API.Controllers;

public class ReportsController : ApiControllerBase
{
    private readonly IReportService _reports;

    public ReportsController(IReportService reports) => _reports = reports;

    /// <summary>Belirtilen ay (varsayılan: bu ay) için PDF ilerleme raporu üretir.</summary>
    [HttpGet("monthly")]
    public async Task<IActionResult> Monthly([FromQuery] int? year, [FromQuery] int? month, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var y = year ?? now.Year;
        var m = month is >= 1 and <= 12 ? month.Value : now.Month;

        var bytes = await _reports.GenerateMonthlyReportAsync(UserId, y, m, ct);
        return File(bytes, "application/pdf", $"fitmetrics-rapor-{y}-{m:00}.pdf");
    }
}
