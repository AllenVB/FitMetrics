using FitMetrics.Domain.Enums;

namespace FitMetrics.Application.DTOs.Profile;

public record UpdateProfileRequest(
    string FullName,
    int Age,
    Gender Gender,
    double HeightCm,
    double CurrentWeightKg,
    ActivityLevel ActivityLevel,
    GoalType GoalType,
    double? TargetWeightKg,
    int? DailyCalorieGoal,
    int? DailyProteinGoal,
    int? DailyWaterGoalMl);
