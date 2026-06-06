using System.Text;
using FitMetrics.Application.DTOs.Ai;
using FitMetrics.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FitMetrics.API.Controllers;

public class AiController : ApiControllerBase
{
    private readonly IAiService _ai;
    private readonly IChatHistoryService _history;

    public AiController(IAiService ai, IChatHistoryService history)
    {
        _ai = ai;
        _history = history;
    }

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

    /// <summary>Kalıcı sohbet geçmişini döndürür (en eskiden yeniye).</summary>
    [HttpGet("chat/history")]
    public async Task<ActionResult<List<ChatMessageDto>>> ChatHistory(CancellationToken ct)
        => Ok(await _history.GetAsync(UserId, ct));

    /// <summary>Sohbet geçmişini temizler.</summary>
    [HttpDelete("chat/history")]
    public async Task<IActionResult> ClearChatHistory(CancellationToken ct)
    {
        await _history.ClearAsync(UserId, ct);
        return NoContent();
    }

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
        var reply = new StringBuilder();
        try
        {
            await foreach (var chunk in _ai.ChatStreamAsync(UserId, request, ct))
            {
                reply.Append(chunk);
                await Response.WriteAsync(chunk, ct);
                await Response.Body.FlushAsync(ct);
            }

            // Akış başarıyla tamamlandı → kullanıcı mesajı + yanıtı kalıcı geçmişe yaz.
            var userMessage = request.Messages.LastOrDefault(m => m.Role == "user")?.Content ?? string.Empty;
            await _history.SaveExchangeAsync(UserId, userMessage, reply.ToString(), ct);
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
