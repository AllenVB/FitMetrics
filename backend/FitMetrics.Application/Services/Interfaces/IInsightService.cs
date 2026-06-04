using FitMetrics.Application.DTOs.Insights;

namespace FitMetrics.Application.Services.Interfaces;

/// <summary>Kullanıcının verilerini analiz edip kişiselleştirilmiş öneriler (AI Insights) üretir.</summary>
public interface IInsightService
{
    Task<InsightsResponse> GenerateAsync(int userId, CancellationToken ct = default);
}
