using FitMetrics.Application.DTOs.Workouts;

namespace FitMetrics.Application.Services.Interfaces;

public interface IWorkoutService
{
    Task<List<ExerciseDto>> GetExercisesAsync(string? search, CancellationToken ct = default);
    Task<WorkoutLogDto> AddLogAsync(int userId, CreateWorkoutLogRequest request, CancellationToken ct = default);
    Task DeleteLogAsync(int userId, int logId, CancellationToken ct = default);
    Task<List<WorkoutLogDto>> GetLogsAsync(int userId, DateTime? from, DateTime? to, CancellationToken ct = default);
}
