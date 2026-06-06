using FitMetrics.Application.DTOs.Ai;

namespace FitMetrics.Application.Services.Interfaces;

/// <summary>
/// Claude destekli üst seviye AI özellikleri: öğün planı oluşturma, doğal dil koçluk,
/// fotoğraftan yemek analizi. Veriyi toplar, prompt'u kurar ve Claude'u çağırır.
/// </summary>
public interface IAiService
{
    bool IsEnabled { get; }
    Task<MealPlanResponse> GenerateMealPlanAsync(int userId, GenerateMealPlanRequest request, CancellationToken ct = default);
    Task<CoachResponse> GenerateCoachingAsync(int userId, CancellationToken ct = default);
    Task<MealPhotoResponse> AnalyzeMealPhotoAsync(AnalyzeMealPhotoRequest request, CancellationToken ct = default);
    Task<ChatResponse> ChatAsync(int userId, ChatRequest request, CancellationToken ct = default);
    IAsyncEnumerable<string> ChatStreamAsync(int userId, ChatRequest request, CancellationToken ct = default);
}
