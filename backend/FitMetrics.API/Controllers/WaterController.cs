using FitMetrics.Application.DTOs.Water;
using FitMetrics.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FitMetrics.API.Controllers;

public class WaterController : ApiControllerBase
{
    private readonly IWaterService _water;

    public WaterController(IWaterService water) => _water = water;

    [HttpGet("today")]
    public async Task<ActionResult<WaterTodayDto>> Today(CancellationToken ct)
        => Ok(await _water.GetTodayAsync(UserId, ct));

    [HttpPost]
    public async Task<ActionResult<WaterTodayDto>> Add(AddWaterRequest request, CancellationToken ct)
        => Ok(await _water.AddAsync(UserId, request.AmountMl, ct));
}
