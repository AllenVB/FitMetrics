using FitMetrics.Application.DTOs.Auth;
using FitMetrics.Application.DTOs.Dashboard;
using FitMetrics.Application.DTOs.Dietitian;
using FitMetrics.Application.DTOs.Insights;
using FitMetrics.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FitMetrics.API.Controllers;

public class DietitianController : ApiControllerBase
{
    private readonly IDietitianService _dietitian;

    public DietitianController(IDietitianService dietitian) => _dietitian = dietitian;

    /// <summary>Mevcut kullanıcıyı diyetisyen rolüne yükseltir.</summary>
    [HttpPost("enroll")]
    public async Task<ActionResult<UserDto>> Enroll(CancellationToken ct)
        => Ok(await _dietitian.EnrollAsync(UserId, ct));

    [HttpGet("clients")]
    public async Task<ActionResult<List<ClientSummaryDto>>> Clients(CancellationToken ct)
        => Ok(await _dietitian.GetClientsAsync(UserId, ct));

    [HttpPost("clients")]
    public async Task<ActionResult<ClientSummaryDto>> AddClient(AddClientRequest request, CancellationToken ct)
        => Ok(await _dietitian.AddClientAsync(UserId, request, ct));

    [HttpDelete("clients/{clientId:int}")]
    public async Task<IActionResult> RemoveClient(int clientId, CancellationToken ct)
    {
        await _dietitian.RemoveClientAsync(UserId, clientId, ct);
        return NoContent();
    }

    [HttpGet("clients/{clientId:int}/dashboard")]
    public async Task<ActionResult<DashboardDto>> ClientDashboard(int clientId, CancellationToken ct)
        => Ok(await _dietitian.GetClientDashboardAsync(UserId, clientId, ct));

    [HttpGet("clients/{clientId:int}/insights")]
    public async Task<ActionResult<InsightsResponse>> ClientInsights(int clientId, CancellationToken ct)
        => Ok(await _dietitian.GetClientInsightsAsync(UserId, clientId, ct));
}
