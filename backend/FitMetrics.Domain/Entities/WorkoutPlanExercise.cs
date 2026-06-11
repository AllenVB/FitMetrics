using FitMetrics.Domain.Common;

namespace FitMetrics.Domain.Entities;

public class WorkoutPlanExercise : BaseEntity
{
    public int WorkoutPlanDayId { get; set; }
    public WorkoutPlanDay Day { get; set; } = null!;

    public int ExerciseId { get; set; }
    public Exercise Exercise { get; set; } = null!;

    public int? Sets { get; set; }
    public int? Reps { get; set; }
    public int? DurationMinutes { get; set; }
    public int SortOrder { get; set; }
}
