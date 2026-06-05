namespace FitMetrics.Application.DTOs.Reports;

public record ReportMuscleStat(string Muscle, int Count);

/// <summary>PDF aylık rapor için derlenmiş veri modeli.</summary>
public record ReportModel(
    string FullName,
    string Period,
    DateTime GeneratedAt,
    string Goal,
    double CurrentWeightKg,
    double? StartWeightKg,
    double? WeightChangeKg,
    double Bmi,
    int CalorieGoal,
    int ProteinGoal,
    int LoggedDays,
    double AvgCalories,
    double AvgProtein,
    int WorkoutCount,
    double CaloriesBurned,
    List<ReportMuscleStat> MuscleBreakdown);
