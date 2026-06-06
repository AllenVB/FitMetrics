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

    [HttpPost("chat")]
    public async Task<ActionResult<ChatResponse>> Chat(ChatRequest request, CancellationToken ct)
        => Ok(await _ai.ChatAsync(UserId, request, ct));

    [HttpPost("chat/stream")]
    public async Task ChatStream(ChatRequest request, CancellationToken ct)
    {
        if (!_ai.IsEnabled)
        {
            Response.StatusCode = 503;
            await Response.WriteAsJsonAsync(new { status = 503, message = "Yapay zekâ özelliği yapılandırılmamış." }, ct);
            return;
        }

        Response.ContentType = "text/plain; charset=utf-8";
        try
        {
            await foreach (var chunk in _ai.ChatStreamAsync(UserId, request, ct))
            {
                await Response.WriteAsync(chunk, ct);
                await Response.Body.FlushAsync(ct);
            }
        }
        catch (OperationCanceledException)
        {
            // istemci bağlantıyı kapattı — sessizce çık
        }
        catch (Exception ex)
        {
            await Response.WriteAsync($"\n[HATA] {ex.Message}", ct);
        }
    }
}
