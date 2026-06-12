using FitMetrics.Application.DTOs.Progress;

namespace FitMetrics.Application.Services.Interfaces;

public interface IBodyMeasurementService
{
    Task<List<BodyMeasurementDto>> GetHistoryAsync(int userId, CancellationToken ct = default);
    Task<BodyMeasurementDto> AddAsync(int userId, CreateBodyMeasurementRequest request, CancellationToken ct = default);
    Task DeleteAsync(int userId, int id, CancellationToken ct = default);
}
