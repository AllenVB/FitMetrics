using FitMetrics.Application.Common.Exceptions;
using FitMetrics.Application.Common.Helpers;
using FitMetrics.Application.Common.Interfaces;
using FitMetrics.Application.DTOs.Auth;
using FitMetrics.Application.DTOs.Dashboard;
using FitMetrics.Application.DTOs.Dietitian;
using FitMetrics.Application.DTOs.Insights;
using FitMetrics.Application.Services.Interfaces;
using FitMetrics.Domain.Entities;
using FitMetrics.Domain.Enums;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace FitMetrics.Application.Services;

public class DietitianService : IDietitianService
{
    private readonly IApplicationDbContext _db;
    private readonly IMapper _mapper;
    private readonly IDashboardService _dashboardService;
    private readonly IInsightService _insightService;

    public DietitianService(
        IApplicationDbContext db,
        IMapper mapper,
        IDashboardService dashboardService,
        IInsightService insightService)
    {
        _db = db;
        _mapper = mapper;
        _dashboardService = dashboardService;
        _insightService = insightService;
    }

    public async Task<UserDto> EnrollAsync(int userId, CancellationToken ct = default)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct)
                   ?? throw new NotFoundException("Kullanıcı", userId);

        user.Role = UserRole.Dietitian;
        await _db.SaveChangesAsync(ct);
        return _mapper.Map<UserDto>(user);
    }

    public async Task<List<ClientSummaryDto>> GetClientsAsync(int dietitianId, CancellationToken ct = default)
    {
        await EnsureDietitianAsync(dietitianId, ct);

        var links = await _db.DietitianClients.AsNoTracking()
            .Where(dc => dc.DietitianId == dietitianId)
            .Include(dc => dc.Client)
            .OrderByDescending(dc => dc.CreatedAt)
            .ToListAsync(ct);

        return links.Select(dc => ToSummary(dc.Client, dc.CreatedAt)).ToList();
    }

    public async Task<ClientSummaryDto> AddClientAsync(int dietitianId, AddClientRequest request, CancellationToken ct = default)
    {
        await EnsureDietitianAsync(dietitianId, ct);

        var email = request.Email.Trim().ToLowerInvariant();
        var client = await _db.Users.FirstOrDefaultAsync(u => u.Email == email, ct)
                     ?? throw new NotFoundException($"'{email}' e-postalı kullanıcı bulunamadı.");

        if (client.Id == dietitianId)
            throw new ConflictException("Kendinizi danışan olarak ekleyemezsiniz.");

        var exists = await _db.DietitianClients
            .AnyAsync(dc => dc.DietitianId == dietitianId && dc.ClientId == client.Id, ct);
        if (exists)
            throw new ConflictException("Bu danışan zaten ekli.");

        var link = new DietitianClient { DietitianId = dietitianId, ClientId = client.Id };
        _db.DietitianClients.Add(link);
        await _db.SaveChangesAsync(ct);

        return ToSummary(client, link.CreatedAt);
    }

    public async Task RemoveClientAsync(int dietitianId, int clientId, CancellationToken ct = default)
    {
        var link = await _db.DietitianClients
            .FirstOrDefaultAsync(dc => dc.DietitianId == dietitianId && dc.ClientId == clientId, ct)
            ?? throw new NotFoundException("Danışan bağı", clientId);

        _db.DietitianClients.Remove(link);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<DashboardDto> GetClientDashboardAsync(int dietitianId, int clientId, CancellationToken ct = default)
    {
        await EnsureLinkedAsync(dietitianId, clientId, ct);
        return await _dashboardService.GetDashboardAsync(clientId, ct);
    }

    public async Task<InsightsResponse> GetClientInsightsAsync(int dietitianId, int clientId, CancellationToken ct = default)
    {
        await EnsureLinkedAsync(dietitianId, clientId, ct);
        return await _insightService.GenerateAsync(clientId, ct);
    }

    // ---- Yetki kontrolleri ----

    private async Task EnsureDietitianAsync(int userId, CancellationToken ct)
    {
        var role = await _db.Users.Where(u => u.Id == userId).Select(u => u.Role).FirstOrDefaultAsync(ct);
        if (role != UserRole.Dietitian)
            throw new UnauthorizedException("Bu işlem için diyetisyen rolü gerekir.");
    }

    private async Task EnsureLinkedAsync(int dietitianId, int clientId, CancellationToken ct)
    {
        var linked = await _db.DietitianClients
            .AnyAsync(dc => dc.DietitianId == dietitianId && dc.ClientId == clientId, ct);
        if (!linked)
            throw new UnauthorizedException("Bu danışana erişim yetkiniz yok.");
    }

    private static ClientSummaryDto ToSummary(User client, DateTime linkedAt) => new(
        client.Id,
        client.FullName,
        client.Email,
        client.GoalType.ToString(),
        client.CurrentWeightKg,
        HealthCalculator.CalculateBmi(client.CurrentWeightKg, client.HeightCm),
        linkedAt);
}
