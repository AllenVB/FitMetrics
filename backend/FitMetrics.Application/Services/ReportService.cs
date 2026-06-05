using FitMetrics.Application.Common.Exceptions;
using FitMetrics.Application.Common.Helpers;
using FitMetrics.Application.Common.Interfaces;
using FitMetrics.Application.DTOs.Reports;
using FitMetrics.Application.Services.Interfaces;
using FitMetrics.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FitMetrics.Application.Services;

public class ReportService : IReportService
{
    private static readonly string[] MonthNames =
    [
        "", "Ocak", "Şubat", "Mart", "Nisan", "Mayıs", "Haziran",
        "Temmuz", "Ağustos", "Eylül", "Ekim", "Kasım", "Aralık"
    ];

    private readonly IApplicationDbContext _db;
    private readonly IPdfReportGenerator _pdf;

    public ReportService(IApplicationDbContext db, IPdfReportGenerator pdf)
    {
        _db = db;
        _pdf = pdf;
    }

    public async Task<byte[]> GenerateMonthlyReportAsync(int userId, int year, int month, CancellationToken ct = default)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct)
                   ?? throw new NotFoundException("Kullanıcı", userId);

        var monthStart = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        var monthEnd = monthStart.AddMonths(1);

        var nutritionLogs = await _db.NutritionLogs
            .Include(n => n.Food)
            .Where(n => n.UserId == userId && n.LoggedAt >= monthStart && n.LoggedAt < monthEnd)
            .ToListAsync(ct);

        var workoutLogs = await _db.WorkoutLogs
            .Include(w => w.Exercise)
            .Where(w => w.UserId == userId && w.PerformedAt >= monthStart && w.PerformedAt < monthEnd)
            .ToListAsync(ct);

        var monthWeights = await _db.WeightEntries.AsNoTracking()
            .Where(w => w.UserId == userId && w.RecordedAt >= monthStart && w.RecordedAt < monthEnd)
            .OrderBy(w => w.RecordedAt)
            .ToListAsync(ct);

        // Beslenme özeti
        var loggedDays = nutritionLogs.Select(n => n.LoggedAt.Date).Distinct().Count();
        double totalCalories = 0, totalProtein = 0;
        foreach (var l in nutritionLogs)
        {
            var factor = l.AmountGrams / 100.0;
            totalCalories += l.Food.CaloriesPer100g * factor;
            totalProtein += l.Food.ProteinPer100g * factor;
        }
        var avgCalories = loggedDays > 0 ? Math.Round(totalCalories / loggedDays) : 0;
        var avgProtein = loggedDays > 0 ? Math.Round(totalProtein / loggedDays) : 0;

        // Antrenman özeti
        var workoutCount = workoutLogs.Count;
        var caloriesBurned = Math.Round(workoutLogs.Sum(w => w.CaloriesBurned), 0);
        var muscleBreakdown = workoutLogs
            .Where(w => w.Exercise.Category == ExerciseCategory.Strength)
            .GroupBy(w => w.Exercise.MuscleGroup)
            .Select(g => new ReportMuscleStat(MuscleName(g.Key), g.Count()))
            .OrderByDescending(s => s.Count)
            .ToList();

        // Kilo
        double? startWeight = monthWeights.Count > 0 ? monthWeights.First().WeightKg : null;
        double? change = startWeight.HasValue ? Math.Round(user.CurrentWeightKg - startWeight.Value, 1) : null;

        var model = new ReportModel(
            user.FullName,
            $"{MonthNames[month]} {year}",
            DateTime.UtcNow,
            GoalText(user.GoalType),
            user.CurrentWeightKg,
            startWeight,
            change,
            HealthCalculator.CalculateBmi(user.CurrentWeightKg, user.HeightCm),
            user.DailyCalorieGoal,
            user.DailyProteinGoal,
            loggedDays,
            avgCalories,
            avgProtein,
            workoutCount,
            caloriesBurned,
            muscleBreakdown);

        return _pdf.Generate(model);
    }

    private static string GoalText(GoalType goal) => goal switch
    {
        GoalType.LoseWeight => "Kilo Vermek",
        GoalType.GainMuscle => "Kas Kazanmak",
        _ => "Kilo Korumak"
    };

    private static string MuscleName(MuscleGroup group) => group switch
    {
        MuscleGroup.Chest => "Göğüs",
        MuscleGroup.Back => "Sırt",
        MuscleGroup.Legs => "Bacak",
        MuscleGroup.Shoulders => "Omuz",
        MuscleGroup.Arms => "Kol",
        MuscleGroup.Core => "Karın",
        MuscleGroup.FullBody => "Tüm Vücut",
        _ => group.ToString()
    };
}
