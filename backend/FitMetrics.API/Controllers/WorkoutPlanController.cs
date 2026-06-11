using FitMetrics.Application.DTOs.Workouts;
using FitMetrics.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FitMetrics.API.Controllers;

[Microsoft.AspNetCore.Mvc.Route("api/workout-plan")]
public class WorkoutPlanController : ApiControllerBase
{
    private readonly IWorkoutPlanService _service;

    public WorkoutPlanController(IWorkoutPlanService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<List<WorkoutPlanSummaryDto>>> GetAll(CancellationToken ct)
        => Ok(await _service.GetAllAsync(UserId, ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<WorkoutPlanDto>> GetById(int id, CancellationToken ct)
        => Ok(await _service.GetByIdAsync(UserId, id, ct));

    [HttpPost]
    public async Task<ActionResult<WorkoutPlanDto>> Create(CreateWorkoutPlanRequest request, CancellationToken ct)
    {
        var result = await _service.CreateAsync(UserId, request, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<WorkoutPlanDto>> Update(int id, CreateWorkoutPlanRequest request, CancellationToken ct)
        => Ok(await _service.UpdateAsync(UserId, id, request, ct));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _service.DeleteAsync(UserId, id, ct);
        return NoContent();
    }
}
