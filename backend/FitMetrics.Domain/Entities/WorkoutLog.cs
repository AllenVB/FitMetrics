using FitMetrics.Domain.Common;

namespace FitMetrics.Domain.Entities;

/// <summary>
/// Tek bir antrenman kaydı. Kardiyo için süre, kuvvet için set/tekrar/ağırlık tutar.
/// </summary>
public class WorkoutLog : BaseEntity
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int ExerciseId { get; set; }
    public Exercise Exercise { get; set; } = null!;

    public int? DurationMinutes { get; set; }
    public int? Sets { get; set; }
    public int? Reps { get; set; }
    public double? WeightKg { get; set; }

    public double CaloriesBurned { get; set; }
    public DateTime PerformedAt { get; set; } = DateTime.UtcNow;
}
