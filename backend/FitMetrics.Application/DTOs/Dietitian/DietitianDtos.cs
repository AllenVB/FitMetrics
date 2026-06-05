namespace FitMetrics.Application.DTOs.Dietitian;

public record AddClientRequest(string Email);

public record ClientSummaryDto(
    int Id,
    string FullName,
    string Email,
    string GoalType,
    double CurrentWeightKg,
    double Bmi,
    DateTime LinkedAt);
