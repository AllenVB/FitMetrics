using FitMetrics.Application.Common.Exceptions;
using FitMetrics.Application.Common.Interfaces;
using FitMetrics.Application.DTOs.Progress;
using FitMetrics.Application.Services.Interfaces;
using FitMetrics.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FitMetrics.Application.Services;

public class BodyMeasurementService : IBodyMeasurementService
{
    private readonly IApplicationDbContext _db;

    public BodyMeasurementService(IApplicationDbContext db) => _db = db;

    public async Task<List<BodyMeasurementDto>> GetHistoryAsync(int userId, CancellationToken ct = default)
    {
        return await _db.BodyMeasurements
            .Where(m => m.UserId == userId)
            .OrderBy(m => m.RecordedAt)
            .Select(m => ToDto(m))
            .ToListAsync(ct);
    }

    public async Task<BodyMeasurementDto> AddAsync(int userId, CreateBodyMeasurementRequest request, CancellationToken ct = default)
    {
        var entry = new BodyMeasurement
        {
            UserId = userId,
            RecordedAt = request.RecordedAt?.ToUniversalTime() ?? DateTime.UtcNow,
            WaistCm = request.WaistCm,
            HipCm = request.HipCm,
            ChestCm = request.ChestCm,
            ArmCm = request.ArmCm,
            NeckCm = request.NeckCm,
            Notes = request.Notes?.Trim(),
        };
        _db.BodyMeasurements.Add(entry);
        await _db.SaveChangesAsync(ct);
        return ToDto(entry);
    }

    public async Task DeleteAsync(int userId, int id, CancellationToken ct = default)
    {
        var entry = await _db.BodyMeasurements
            .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId, ct)
            ?? throw new NotFoundException("Ölçüm", id);
        _db.BodyMeasurements.Remove(entry);
        await _db.SaveChangesAsync(ct);
    }

    private static BodyMeasurementDto ToDto(BodyMeasurement m) =>
        new(m.Id, m.RecordedAt, m.WaistCm, m.HipCm, m.ChestCm, m.ArmCm, m.NeckCm, m.Notes);
}
