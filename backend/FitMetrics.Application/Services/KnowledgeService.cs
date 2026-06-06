using FitMetrics.Application.Common.Exceptions;
using FitMetrics.Application.Common.Interfaces;
using FitMetrics.Application.DTOs.Knowledge;
using FitMetrics.Application.Services.Interfaces;
using FitMetrics.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FitMetrics.Application.Services;

public class KnowledgeService : IKnowledgeService
{
    private readonly IApplicationDbContext _db;

    public KnowledgeService(IApplicationDbContext db) => _db = db;

    public async Task<List<KnowledgeEntryDto>> GetAllAsync(CancellationToken ct = default)
    {
        var entries = await _db.KnowledgeEntries.AsNoTracking()
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync(ct);
        return entries.Select(ToDto).ToList();
    }

    public async Task<KnowledgeEntryDto> CreateAsync(CreateKnowledgeEntryRequest request, CancellationToken ct = default)
    {
        var entry = new KnowledgeEntry
        {
            Question = request.Question.Trim(),
            Answer = request.Answer.Trim()
        };
        _db.KnowledgeEntries.Add(entry);
        await _db.SaveChangesAsync(ct);
        return ToDto(entry);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var entry = await _db.KnowledgeEntries.FirstOrDefaultAsync(e => e.Id == id, ct)
                    ?? throw new NotFoundException("Bilgi kaydı", id);
        _db.KnowledgeEntries.Remove(entry);
        await _db.SaveChangesAsync(ct);
    }

    private static KnowledgeEntryDto ToDto(KnowledgeEntry e) => new(e.Id, e.Question, e.Answer, e.CreatedAt);
}
