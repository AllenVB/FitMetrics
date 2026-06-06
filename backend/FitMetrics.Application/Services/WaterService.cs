using FitMetrics.Application.Common.Exceptions;
using FitMetrics.Application.Common.Interfaces;
using FitMetrics.Application.DTOs.Water;
using FitMetrics.Application.Services.Interfaces;
using FitMetrics.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FitMetrics.Application.Services;

public class WaterService : IWaterService
{
    private readonly IApplicationDbContext _db;

    public WaterService(IApplicationDbContext db) => _db = db;

    public async Task<WaterTodayDto> GetTodayAsync(int userId, CancellationToken ct = default)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct)
                   ?? throw new NotFoundException("Kullanıcı", userId);
        return new WaterTodayDto(await SumTodayAsync(userId, ct), user.DailyWaterGoalMl);
    }

    public async Task<WaterTodayDto> AddAsync(int userId, int amountMl, CancellationToken ct = default)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct)
                   ?? throw new NotFoundException("Kullanıcı", userId);

        var amount = Math.Clamp(amountMl, 1, 5000);
        _db.WaterLogs.Add(new WaterLog { UserId = userId, AmountMl = amount, LoggedAt = DateTime.UtcNow });
        await _db.SaveChangesAsync(ct);

        return new WaterTodayDto(await SumTodayAsync(userId, ct), user.DailyWaterGoalMl);
    }

    private async Task<int> SumTodayAsync(int userId, CancellationToken ct)
    {
        var dayStart = DateTime.UtcNow.Date;
        var dayEnd = dayStart.AddDays(1);
        return await _db.WaterLogs
            .Where(w => w.UserId == userId && w.LoggedAt >= dayStart && w.LoggedAt < dayEnd)
            .SumAsync(w => w.AmountMl, ct);
    }
}
