using FitMetrics.Domain.Common;

namespace FitMetrics.Domain.Entities;

/// <summary>
/// Programdaki bir gün. DayIndex: 0=Pazartesi … 6=Pazar
/// </summary>
public class WorkoutPlanDay : BaseEntity
{
    public int WorkoutPlanId { get; set; }
    public WorkoutPlan WorkoutPlan { get; set; } = null!;

    public int DayIndex { get; set; } // 0–6

    public ICollection<WorkoutPlanExercise> Exercises { get; set; } = new List<WorkoutPlanExercise>();
}
