using FitMetrics.Application.DTOs.Weight;

namespace FitMetrics.Application.Services.Interfaces;

public interface IWeightService
{
    Task<WeightEntryDto> AddEntryAsync(int userId, CreateWeightEntryRequest request, CancellationToken ct = default);
    Task<List<WeightEntryDto>> GetHistoryAsync(int userId, CancellationToken ct = default);
    Task DeleteEntryAsync(int userId, int entryId, CancellationToken ct = default);
}
