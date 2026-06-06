using FitMetrics.Application.DTOs.Knowledge;

namespace FitMetrics.Application.Services.Interfaces;

/// <summary>Sohbet asistanının temel alacağı onaylı soru/cevap bilgi tabanı yönetimi.</summary>
public interface IKnowledgeService
{
    Task<List<KnowledgeEntryDto>> GetAllAsync(CancellationToken ct = default);
    Task<KnowledgeEntryDto> CreateAsync(CreateKnowledgeEntryRequest request, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
