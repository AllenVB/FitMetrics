using FitMetrics.Application.DTOs.Dashboard;

namespace FitMetrics.Application.Services.Interfaces;

public interface IDashboardService
{
    Task<DashboardDto> GetDashboardAsync(int userId, CancellationToken ct = default);
}
