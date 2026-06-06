using FitMetrics.Application.DTOs.Ai;

namespace FitMetrics.Application.Services.Interfaces;

/// <summary>AI Asistan sohbetinin kullanıcı bazında kalıcı geçmişi.</summary>
public interface IChatHistoryService
{
    Task<List<ChatMessageDto>> GetAsync(int userId, CancellationToken ct = default);
    Task SaveExchangeAsync(int userId, string userMessage, string assistantMessage, CancellationToken ct = default);
    Task ClearAsync(int userId, CancellationToken ct = default);
}
