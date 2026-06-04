using FitMetrics.Domain.Common;
using FitMetrics.Domain.Enums;

namespace FitMetrics.Domain.Entities;

public class User : BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    // Profil / vücut bilgileri
    public int Age { get; set; }
    public Gender Gender { get; set; }
    public double HeightCm { get; set; }
    public double CurrentWeightKg { get; set; }
    public ActivityLevel ActivityLevel { get; set; } = ActivityLevel.Moderate;

    // Hedef
    public GoalType GoalType { get; set; } = GoalType.MaintainWeight;
    public double? TargetWeightKg { get; set; }

    // Günlük hedefler (kayıtta hesaplanır, profil üzerinden güncellenebilir)
    public int DailyCalorieGoal { get; set; }
    public int DailyProteinGoal { get; set; }
    public int DailyWaterGoalMl { get; set; } = 2500;

    // İlişkiler
    public ICollection<NutritionLog> NutritionLogs { get; set; } = new List<NutritionLog>();
    public ICollection<WorkoutLog> WorkoutLogs { get; set; } = new List<WorkoutLog>();
    public ICollection<WeightEntry> WeightEntries { get; set; } = new List<WeightEntry>();
}
