using FitMetrics.Application.DTOs.Weight;
using FitMetrics.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FitMetrics.API.Controllers;

public class WeightController : ApiControllerBase
{
    private readonly IWeightService _weightService;

    public WeightController(IWeightService weightService) => _weightService = weightService;

    [HttpPost]
    public async Task<ActionResult<WeightEntryDto>> Add(CreateWeightEntryRequest request, CancellationToken ct)
        => Ok(await _weightService.AddEntryAsync(UserId, request, ct));

    [HttpGet]
    public async Task<ActionResult<List<WeightEntryDto>>> History(CancellationToken ct)
        => Ok(await _weightService.GetHistoryAsync(UserId, ct));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _weightService.DeleteEntryAsync(UserId, id, ct);
        return NoContent();
    }
}
