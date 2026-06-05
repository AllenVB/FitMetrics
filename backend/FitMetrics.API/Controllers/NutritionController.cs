using FitMetrics.Application.DTOs.Nutrition;
using FitMetrics.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FitMetrics.API.Controllers;

public class NutritionController : ApiControllerBase
{
    private readonly INutritionService _nutritionService;

    public NutritionController(INutritionService nutritionService) => _nutritionService = nutritionService;

    [HttpGet("foods")]
    public async Task<ActionResult<List<FoodDto>>> GetFoods([FromQuery] string? search, CancellationToken ct)
        => Ok(await _nutritionService.GetFoodsAsync(search, ct));

    [HttpPost("foods")]
    public async Task<ActionResult<FoodDto>> CreateFood(CreateFoodRequest request, CancellationToken ct)
        => Ok(await _nutritionService.CreateFoodAsync(request, ct));

    [HttpPost("logs")]
    public async Task<ActionResult<NutritionLogDto>> AddLog(CreateNutritionLogRequest request, CancellationToken ct)
        => Ok(await _nutritionService.AddLogAsync(UserId, request, ct));

    [HttpDelete("logs/{id:int}")]
    public async Task<IActionResult> DeleteLog(int id, CancellationToken ct)
    {
        await _nutritionService.DeleteLogAsync(UserId, id, ct);
        return NoContent();
    }

    [HttpGet("summary")]
    public async Task<ActionResult<DailyNutritionSummaryDto>> Summary([FromQuery] DateTime? date, CancellationToken ct)
        => Ok(await _nutritionService.GetDailySummaryAsync(UserId, date ?? DateTime.UtcNow.Date, ct));

    [HttpGet("barcode/{code}")]
    public async Task<ActionResult<BarcodeLookupResult>> Barcode(string code, CancellationToken ct)
        => Ok(await _nutritionService.LookupBarcodeAsync(code, ct));
}
