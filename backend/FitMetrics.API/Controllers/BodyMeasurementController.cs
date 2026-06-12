using FitMetrics.Application.DTOs.Progress;
using FitMetrics.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FitMetrics.API.Controllers;

[Route("api/measurements")]
public class BodyMeasurementController : ApiControllerBase
{
    private readonly IBodyMeasurementService _service;

    public BodyMeasurementController(IBodyMeasurementService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<List<BodyMeasurementDto>>> GetHistory(CancellationToken ct)
        => Ok(await _service.GetHistoryAsync(UserId, ct));

    [HttpPost]
    public async Task<ActionResult<BodyMeasurementDto>> Add(CreateBodyMeasurementRequest request, CancellationToken ct)
        => Ok(await _service.AddAsync(UserId, request, ct));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _service.DeleteAsync(UserId, id, ct);
        return NoContent();
    }
}
