using FitMetrics.Application.DTOs.Nutrition;

namespace FitMetrics.Application.Services.Interfaces;

public interface INutritionService
{
    Task<List<FoodDto>> GetFoodsAsync(string? search, CancellationToken ct = default);
    Task<FoodDto> CreateFoodAsync(CreateFoodRequest request, CancellationToken ct = default);
    Task<NutritionLogDto> AddLogAsync(int userId, CreateNutritionLogRequest request, CancellationToken ct = default);
    Task DeleteLogAsync(int userId, int logId, CancellationToken ct = default);
    Task<DailyNutritionSummaryDto> GetDailySummaryAsync(int userId, DateTime date, CancellationToken ct = default);
}
