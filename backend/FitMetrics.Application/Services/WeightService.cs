using FitMetrics.Application.Common.Exceptions;
using FitMetrics.Application.Common.Interfaces;
using FitMetrics.Application.DTOs.Weight;
using FitMetrics.Application.Services.Interfaces;
using FitMetrics.Domain.Entities;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace FitMetrics.Application.Services;

public class WeightService : IWeightService
{
    private readonly IApplicationDbContext _db;
    private readonly IMapper _mapper;

    public WeightService(IApplicationDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<WeightEntryDto> AddEntryAsync(int userId, CreateWeightEntryRequest request, CancellationToken ct = default)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct)
                   ?? throw new NotFoundException("Kullanıcı", userId);

        var entry = new WeightEntry
        {
            UserId = userId,
            WeightKg = request.WeightKg,
            BodyFatPercentage = request.BodyFatPercentage,
            RecordedAt = request.RecordedAt ?? DateTime.UtcNow
        };

        _db.WeightEntries.Add(entry);

        // Bu kayıt en güncel tarihliyse kullanıcının "mevcut kilo" değerini güncelle
        var latestExisting = await _db.WeightEntries
            .Where(w => w.UserId == userId)
            .OrderByDescending(w => w.RecordedAt)
            .Select(w => (DateTime?)w.RecordedAt)
            .FirstOrDefaultAsync(ct);

        if (latestExisting is null || entry.RecordedAt >= latestExisting)
            user.CurrentWeightKg = request.WeightKg;

        await _db.SaveChangesAsync(ct);
        return _mapper.Map<WeightEntryDto>(entry);
    }

    public async Task<List<WeightEntryDto>> GetHistoryAsync(int userId, CancellationToken ct = default)
    {
        var entries = await _db.WeightEntries.AsNoTracking()
            .Where(w => w.UserId == userId)
            .OrderBy(w => w.RecordedAt)
            .ToListAsync(ct);

        return entries.Select(e => _mapper.Map<WeightEntryDto>(e)).ToList();
    }

    public async Task DeleteEntryAsync(int userId, int entryId, CancellationToken ct = default)
    {
        var entry = await _db.WeightEntries.FirstOrDefaultAsync(w => w.Id == entryId && w.UserId == userId, ct)
                    ?? throw new NotFoundException("Kilo kaydı", entryId);

        _db.WeightEntries.Remove(entry);
        await _db.SaveChangesAsync(ct);
    }
}
