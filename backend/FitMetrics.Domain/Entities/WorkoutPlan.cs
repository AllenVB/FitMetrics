using FitMetrics.Domain.Common;

namespace FitMetrics.Domain.Entities;

public class WorkoutPlan : BaseEntity
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public string Name { get; set; } = string.Empty;

    public ICollection<WorkoutPlanDay> Days { get; set; } = new List<WorkoutPlanDay>();
}
