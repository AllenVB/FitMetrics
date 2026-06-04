using FitMetrics.Application.DTOs.Insights;
using FitMetrics.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FitMetrics.API.Controllers;

public class InsightsController : ApiControllerBase
{
    private readonly IInsightService _insightService;

    public InsightsController(IInsightService insightService) => _insightService = insightService;

    [HttpGet]
    public async Task<ActionResult<InsightsResponse>> Get(CancellationToken ct)
        => Ok(await _insightService.GenerateAsync(UserId, ct));
}
