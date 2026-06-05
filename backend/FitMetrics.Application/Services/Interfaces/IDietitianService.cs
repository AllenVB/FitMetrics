using FitMetrics.Application.DTOs.Auth;
using FitMetrics.Application.DTOs.Dashboard;
using FitMetrics.Application.DTOs.Dietitian;
using FitMetrics.Application.DTOs.Insights;

namespace FitMetrics.Application.Services.Interfaces;

/// <summary>Diyetisyen/antrenör paneli: danışan yönetimi ve danışan verilerine (yetkili) erişim.</summary>
public interface IDietitianService
{
    Task<UserDto> EnrollAsync(int userId, CancellationToken ct = default);
    Task<List<ClientSummaryDto>> GetClientsAsync(int dietitianId, CancellationToken ct = default);
    Task<ClientSummaryDto> AddClientAsync(int dietitianId, AddClientRequest request, CancellationToken ct = default);
    Task RemoveClientAsync(int dietitianId, int clientId, CancellationToken ct = default);
    Task<DashboardDto> GetClientDashboardAsync(int dietitianId, int clientId, CancellationToken ct = default);
    Task<InsightsResponse> GetClientInsightsAsync(int dietitianId, int clientId, CancellationToken ct = default);
}
