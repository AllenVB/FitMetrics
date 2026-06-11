namespace FitMetrics.Application.DTOs.Workouts;

// ── Okuma ────────────────────────────────────────────────────────────────────

public record WorkoutPlanSummaryDto(
    int Id,
    string Name,
    DateTime CreatedAt,
    int TotalExercises);

public record WorkoutPlanDto(
    int Id,
    string Name,
    DateTime CreatedAt,
    List<WorkoutPlanDayDto> Days);

public record WorkoutPlanDayDto(
    int Id,
    int DayIndex,
    List<WorkoutPlanExerciseDto> Exercises);

public record WorkoutPlanExerciseDto(
    int Id,
    int ExerciseId,
    string ExerciseName,
    string MuscleGroup,
    string Category,
    int? Sets,
    int? Reps,
    int? DurationMinutes,
    int SortOrder);

// ── Yazma ────────────────────────────────────────────────────────────────────

public record CreateWorkoutPlanRequest(
    string Name,
    List<CreateWorkoutPlanDayRequest> Days);

public record CreateWorkoutPlanDayRequest(
    int DayIndex,
    List<CreateWorkoutPlanExerciseRequest> Exercises);

public record CreateWorkoutPlanExerciseRequest(
    int ExerciseId,
    int? Sets,
    int? Reps,
    int? DurationMinutes,
    int SortOrder);
