using FitMetrics.Application.DTOs.Ai;
using FitMetrics.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FitMetrics.API.Controllers;

public class AiController : ApiControllerBase
{
    private readonly IAiService _ai;

    public AiController(IAiService ai) => _ai = ai;

    /// <summary>AI özellikleri etkin mi (API anahtarı tanımlı mı)?</summary>
    [HttpGet("status")]
    public ActionResult<object> Status() => Ok(new { enabled = _ai.IsEnabled });

    [HttpPost("meal-plan")]
    public async Task<ActionResult<MealPlanResponse>> MealPlan(GenerateMealPlanRequest request, CancellationToken ct)
        => Ok(await _ai.GenerateMealPlanAsync(UserId, request, ct));

    [HttpGet("coach")]
    public async Task<ActionResult<CoachResponse>> Coach(CancellationToken ct)
        => Ok(await _ai.GenerateCoachingAsync(UserId, ct));

    [HttpPost("analyze-meal-photo")]
    public async Task<ActionResult<MealPhotoResponse>> AnalyzePhoto(AnalyzeMealPhotoRequest request, CancellationToken ct)
        => Ok(await _ai.AnalyzeMealPhotoAsync(request, ct));
}
