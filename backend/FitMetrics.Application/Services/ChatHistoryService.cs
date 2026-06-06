using FitMetrics.Application.Common.Interfaces;
using FitMetrics.Application.DTOs.Ai;
using FitMetrics.Application.Services.Interfaces;
using FitMetrics.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FitMetrics.Application.Services;

public class ChatHistoryService : IChatHistoryService
{
    private const int MaxMessages = 100;
    private readonly IApplicationDbContext _db;

    public ChatHistoryService(IApplicationDbContext db) => _db = db;

    public async Task<List<ChatMessageDto>> GetAsync(int userId, CancellationToken ct = default)
        => await _db.ChatMessages
            .Where(m => m.UserId == userId)
            .OrderBy(m => m.CreatedAt).ThenBy(m => m.Id)
            .Select(m => new ChatMessageDto(m.Role, m.Content))
            .ToListAsync(ct);

    public async Task SaveExchangeAsync(int userId, string userMessage, string assistantMessage, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(userMessage) || string.IsNullOrWhiteSpace(assistantMessage)) return;

        var now = DateTime.UtcNow;
        _db.ChatMessages.Add(new ChatMessage { UserId = userId, Role = "user", Content = Trunc(userMessage), CreatedAt = now });
        _db.ChatMessages.Add(new ChatMessage { UserId = userId, Role = "assistant", Content = Trunc(assistantMessage), CreatedAt = now.AddMilliseconds(1) });
        await _db.SaveChangesAsync(ct);

        // Geçmişin sınırsız büyümesini engelle: en eskileri buda.
        var count = await _db.ChatMessages.CountAsync(m => m.UserId == userId, ct);
        if (count > MaxMessages)
        {
            var toRemove = await _db.ChatMessages
                .Where(m => m.UserId == userId)
                .OrderBy(m => m.CreatedAt).ThenBy(m => m.Id)
                .Take(count - MaxMessages)
                .ToListAsync(ct);
            _db.ChatMessages.RemoveRange(toRemove);
            await _db.SaveChangesAsync(ct);
        }
    }

    public async Task ClearAsync(int userId, CancellationToken ct = default)
    {
        var msgs = await _db.ChatMessages.Where(m => m.UserId == userId).ToListAsync(ct);
        _db.ChatMessages.RemoveRange(msgs);
        await _db.SaveChangesAsync(ct);
    }

    private static string Trunc(string s) => s.Length > 8000 ? s[..8000] : s;
}
