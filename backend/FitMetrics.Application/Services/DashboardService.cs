using FitMetrics.Application.Common.Exceptions;
using FitMetrics.Application.Common.Helpers;
using FitMetrics.Application.Common.Interfaces;
using FitMetrics.Application.DTOs.Dashboard;
using FitMetrics.Application.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FitMetrics.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IApplicationDbContext _db;
    private readonly INutritionService _nutritionService;

    public DashboardService(IApplicationDbContext db, INutritionService nutritionService)
    {
        _db = db;
        _nutritionService = nutritionService;
    }

    public async Task<DashboardDto> GetDashboardAsync(int userId, CancellationToken ct = default)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct)
                   ?? throw new NotFoundException("Kullanıcı", userId);

        var today = DateTime.UtcNow.Date;
        var todaySummary = await _nutritionService.GetDailySummaryAsync(userId, today, ct);

        // Son 14 günün kalori trendi (alım vs. hedef vs. yakım)
        const int trendDays = 14;
        var trendStart = today.AddDays(-(trendDays - 1));
        var trendEnd = today.AddDays(1);

        var nutritionLogs = await _db.NutritionLogs
            .Include(n => n.Food)
            .Where(n => n.UserId == userId && n.LoggedAt >= trendStart && n.LoggedAt < trendEnd)
            .ToListAsync(ct);

        var workoutLogs = await _db.WorkoutLogs
            .Where(w => w.UserId == userId && w.PerformedAt >= trendStart && w.PerformedAt < trendEnd)
            .ToListAsync(ct);

        var calorieTrend = new List<DailyCaloriePointDto>(trendDays);
        for (var i = 0; i < trendDays; i++)
        {
            var day = trendStart.AddDays(i);
            var dayEnd = day.AddDays(1);

            var calories = nutritionLogs
                .Where(n => n.LoggedAt >= day && n.LoggedAt < dayEnd)
                .Sum(n => n.Food.CaloriesPer100g * n.AmountGrams / 100.0);

            var burned = workoutLogs
                .Where(w => w.PerformedAt >= day && w.PerformedAt < dayEnd)
                .Sum(w => w.CaloriesBurned);

            calorieTrend.Add(new DailyCaloriePointDto(day, Math.Round(calories, 1), user.DailyCalorieGoal, Math.Round(burned, 1)));
        }

        // Kilo trendi (en fazla son 60 kayıt)
        var weightTrend = await _db.WeightEntries.AsNoTracking()
            .Where(w => w.UserId == userId)
            .OrderBy(w => w.RecordedAt)
            .Select(w => new WeightPointDto(w.RecordedAt, w.WeightKg, w.BodyFatPercentage))
            .ToListAsync(ct);

        if (weightTrend.Count > 60)
            weightTrend = weightTrend.Skip(weightTrend.Count - 60).ToList();

        // Bu hafta (son 7 gün) antrenman özeti
        var weekStart = today.AddDays(-6);
        var workoutsThisWeek = workoutLogs.Count(w => w.PerformedAt >= weekStart);
        var burnedThisWeek = workoutLogs.Where(w => w.PerformedAt >= weekStart).Sum(w => w.CaloriesBurned);

        var macros = new MacroBreakdownDto(todaySummary.TotalProtein, todaySummary.TotalCarbs, todaySummary.TotalFat);
        var bmi = HealthCalculator.CalculateBmi(user.CurrentWeightKg, user.HeightCm);

        return new DashboardDto(
            todaySummary,
            user.DailyWaterGoalMl,
            user.CurrentWeightKg,
            user.TargetWeightKg,
            bmi,
            workoutsThisWeek,
            Math.Round(burnedThisWeek, 1),
            macros,
            calorieTrend,
            weightTrend);
    }
}
