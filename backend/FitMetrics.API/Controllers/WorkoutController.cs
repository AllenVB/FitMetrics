using FitMetrics.Application.DTOs.Workouts;
using FitMetrics.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FitMetrics.API.Controllers;

public class WorkoutController : ApiControllerBase
{
    private readonly IWorkoutService _workoutService;

    public WorkoutController(IWorkoutService workoutService) => _workoutService = workoutService;

    [HttpGet("exercises")]
    public async Task<ActionResult<List<ExerciseDto>>> GetExercises([FromQuery] string? search, CancellationToken ct)
        => Ok(await _workoutService.GetExercisesAsync(search, ct));

    [HttpPost("logs")]
    public async Task<ActionResult<WorkoutLogDto>> AddLog(CreateWorkoutLogRequest request, CancellationToken ct)
        => Ok(await _workoutService.AddLogAsync(UserId, request, ct));

    [HttpDelete("logs/{id:int}")]
    public async Task<IActionResult> DeleteLog(int id, CancellationToken ct)
    {
        await _workoutService.DeleteLogAsync(UserId, id, ct);
        return NoContent();
    }

    [HttpGet("logs")]
    public async Task<ActionResult<List<WorkoutLogDto>>> GetLogs([FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken ct)
        => Ok(await _workoutService.GetLogsAsync(UserId, from, to, ct));
}
