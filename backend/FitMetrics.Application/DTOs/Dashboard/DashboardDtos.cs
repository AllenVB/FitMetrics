using FitMetrics.Application.DTOs.Nutrition;

namespace FitMetrics.Application.DTOs.Dashboard;

public record DailyCaloriePointDto(DateTime Date, double Calories, int Goal, double BurnedCalories);

public record WeightPointDto(DateTime Date, double WeightKg, double? BodyFatPercentage);

public record MacroBreakdownDto(double Protein, double Carbs, double Fat);

public record DashboardDto(
    DailyNutritionSummaryDto Today,
    int WaterGoalMl,
    int WaterIntakeMl,
    double? CurrentWeightKg,
    double? TargetWeightKg,
    double Bmi,
    int WorkoutsThisWeek,
    double CaloriesBurnedThisWeek,
    MacroBreakdownDto TodayMacros,
    List<DailyCaloriePointDto> CalorieTrend,
    List<WeightPointDto> WeightTrend);
