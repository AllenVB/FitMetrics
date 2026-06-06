using FitMetrics.Application.DTOs.Water;

namespace FitMetrics.Application.Services.Interfaces;

public interface IWaterService
{
    Task<WaterTodayDto> GetTodayAsync(int userId, CancellationToken ct = default);
    Task<WaterTodayDto> AddAsync(int userId, int amountMl, CancellationToken ct = default);
}
