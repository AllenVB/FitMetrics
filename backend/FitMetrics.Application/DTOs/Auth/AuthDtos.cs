using FitMetrics.Domain.Enums;

namespace FitMetrics.Application.DTOs.Auth;

public record RegisterRequest(
    string FullName,
    string Email,
    string Password,
    int Age,
    Gender Gender,
    double HeightCm,
    double CurrentWeightKg,
    ActivityLevel ActivityLevel,
    GoalType GoalType,
    double? TargetWeightKg);

public record LoginRequest(string Email, string Password);

public record UserDto(
    int Id,
    string FullName,
    string Email,
    int Age,
    Gender Gender,
    double HeightCm,
    double CurrentWeightKg,
    ActivityLevel ActivityLevel,
    GoalType GoalType,
    double? TargetWeightKg,
    int DailyCalorieGoal,
    int DailyProteinGoal,
    int DailyWaterGoalMl,
    double Bmi);

public record AuthResponse(string Token, DateTime ExpiresAt, UserDto User);
