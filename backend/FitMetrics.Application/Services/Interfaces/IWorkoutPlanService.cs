using FitMetrics.Application.DTOs.Workouts;

namespace FitMetrics.Application.Services.Interfaces;

public interface IWorkoutPlanService
{
    Task<List<WorkoutPlanSummaryDto>> GetAllAsync(int userId, CancellationToken ct = default);
    Task<WorkoutPlanDto> GetByIdAsync(int userId, int planId, CancellationToken ct = default);
    Task<WorkoutPlanDto> CreateAsync(int userId, CreateWorkoutPlanRequest request, CancellationToken ct = default);
    Task<WorkoutPlanDto> UpdateAsync(int userId, int planId, CreateWorkoutPlanRequest request, CancellationToken ct = default);
    Task DeleteAsync(int userId, int planId, CancellationToken ct = default);
}
