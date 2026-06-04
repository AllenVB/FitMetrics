using FitMetrics.Domain.Common;
using FitMetrics.Domain.Enums;

namespace FitMetrics.Domain.Entities;

public class Exercise : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public ExerciseCategory Category { get; set; }
    public MuscleGroup MuscleGroup { get; set; }

    /// <summary>Dakika başına yakılan tahmini kalori (süre bazlı hesap için).</summary>
    public double CaloriesBurnedPerMinute { get; set; }

    public ICollection<WorkoutLog> WorkoutLogs { get; set; } = new List<WorkoutLog>();
}
