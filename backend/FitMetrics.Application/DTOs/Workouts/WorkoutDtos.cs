using FitMetrics.Domain.Enums;

namespace FitMetrics.Application.DTOs.Workouts;

public record ExerciseDto(
    int Id,
    string Name,
    ExerciseCategory Category,
    MuscleGroup MuscleGroup,
    double CaloriesBurnedPerMinute);

public record CreateWorkoutLogRequest(
    int ExerciseId,
    int? DurationMinutes,
    int? Sets,
    int? Reps,
    double? WeightKg,
    DateTime? PerformedAt);

public record WorkoutLogDto(
    int Id,
    int ExerciseId,
    string ExerciseName,
    ExerciseCategory Category,
    MuscleGroup MuscleGroup,
    int? DurationMinutes,
    int? Sets,
    int? Reps,
    double? WeightKg,
    double CaloriesBurned,
    DateTime PerformedAt);
